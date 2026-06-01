using SalesPoint.Application.DTOs.Sales;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class SaleService : ISaleService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaleService(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        IStockMovementRepository stockMovementRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _stockMovementRepository = stockMovementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SaleDto> CreateAsync(SaleCreateDto request)
    {
        if (request is null)
            throw new DomainException("La venta no puede estar vacía.");

        ApplicationValidator.Positive(request.CustomerId, "El cliente");

        if (request.Details is null || !request.Details.Any())
            throw new DomainException("La venta debe tener al menos un producto.");

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId)
            ?? throw new DomainException("El cliente seleccionado no existe.");

        var sale = new Invoice(request.CustomerId);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var detail in request.Details)
            {
                ApplicationValidator.Positive(detail.ProductId, "El producto");
                ApplicationValidator.Positive(detail.Quantity, "La cantidad");

                var product = await _productRepository.GetByIdAsync(detail.ProductId)
                    ?? throw new DomainException("Uno de los productos seleccionados no existe.");

                sale.AddDetail(product, detail.Quantity);

                await _stockMovementRepository.CreateAsync(new StockMovement(
                    product.Id,
                    "OUT",
                    detail.Quantity * -1,
                    product.Stock,
                    "Venta registrada"
                ));
            }

            var created = await _saleRepository.CreateAsync(sale);

            await _unitOfWork.CommitAsync();

            return Map(created, $"{customer.FirstName} {customer.LastName}");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<SaleDto> ConfirmAsync(int id)
    {
        var sale = await _saleRepository.GetByIdAsync(id)
            ?? throw new DomainException("Venta no encontrada.");

        sale.Confirm();

        await _saleRepository.UpdateAsync(sale);

        var customer = await _customerRepository.GetByIdAsync(sale.CustomerId);

        return Map(
            sale,
            customer is null
                ? "Cliente no encontrado"
                : $"{customer.FirstName} {customer.LastName}"
        );
    }

    public async Task<SaleDto> CancelAsync(int id)
    {
        var sale = await _saleRepository.GetByIdAsync(id)
            ?? throw new DomainException("Venta no encontrada.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var detail in sale.Details)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);

                if (product is not null)
                {
                    product.IncreaseStock(detail.Quantity);

                    await _productRepository.UpdateAsync(product);

                    await _stockMovementRepository.CreateAsync(new StockMovement(
                        product.Id,
                        "IN",
                        detail.Quantity,
                        product.Stock,
                        "Cancelación de venta"
                    ));
                }
            }

            sale.Cancel();

            await _saleRepository.UpdateAsync(sale);

            await _unitOfWork.CommitAsync();

            var customer = await _customerRepository.GetByIdAsync(sale.CustomerId);

            return Map(
                sale,
                customer is null
                    ? "Cliente no encontrado"
                    : $"{customer.FirstName} {customer.LastName}"
            );
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<List<SaleHistoryDto>> GetHistoryAsync()
    {
        var sales = await _saleRepository.GetAllAsync();
        var result = new List<SaleHistoryDto>();

        foreach (var sale in sales)
        {
            var customer = await _customerRepository.GetByIdAsync(sale.CustomerId);

            result.Add(new SaleHistoryDto
            {
                Id = sale.Id,
                InvoiceNumber = sale.InvoiceNumber,
                CustomerId = sale.CustomerId,
                CustomerName = customer is null
                    ? "Cliente no encontrado"
                    : $"{customer.FirstName} {customer.LastName}",
                Date = sale.Date,
                Status = sale.Status,
                Total = sale.Total
            });
        }

        return result;
    }

    public async Task<SaleDto?> GetInvoiceByIdAsync(int id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);

        if (sale is null)
            return null;

        var customer = await _customerRepository.GetByIdAsync(sale.CustomerId);

        return Map(
            sale,
            customer is null
                ? "Cliente no encontrado"
                : $"{customer.FirstName} {customer.LastName}"
        );
    }

    private static SaleDto Map(Invoice sale, string customerName)
    {
        return new SaleDto
        {
            Id = sale.Id,
            InvoiceNumber = sale.InvoiceNumber,
            CustomerId = sale.CustomerId,
            CustomerName = customerName,
            Date = sale.Date,
            Status = sale.Status,
            Subtotal = sale.Subtotal,
            Tax = sale.Tax,
            Total = sale.Total,
            Details = sale.Details.Select(detail => new SaleDetailDto
            {
                ProductId = detail.ProductId,
                ProductName = detail.ProductName,
                Price = detail.Price,
                Quantity = detail.Quantity,
                Subtotal = detail.Subtotal
            }).ToList()
        };
    }
}