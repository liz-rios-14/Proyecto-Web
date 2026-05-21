using SalesPoint.Application.DTOs.Invoices;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class InvoiceService : IInvoiceService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceService(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IInvoiceRepository invoiceRepository)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request)
    {
        if (request is null)
            throw new DomainException("La factura no puede estar vacía.");

        if (request.Details is null || !request.Details.Any())
            throw new DomainException("La factura debe tener al menos un producto.");

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);

        if (customer is null)
            throw new DomainException("El cliente seleccionado no existe.");

        var invoice = new Invoice(request.CustomerId);

        foreach (var detail in request.Details)
        {
            var product = await _productRepository.GetByIdAsync(detail.ProductId);

            if (product is null)
                throw new DomainException("Uno de los productos seleccionados no existe.");

            invoice.AddDetail(product, detail.Quantity);

            // OJO:
            // No guardar aquí.
            // El stock y la factura se guardan juntos al final.
        }

        var createdInvoice = await _invoiceRepository.CreateAsync(invoice);

        var dto = MapInvoiceToDto(createdInvoice);

        dto.CustomerName = $"{customer.FirstName} {customer.LastName}";
        dto.InvoiceNumber = createdInvoice.InvoiceNumber;
        dto.CustomerPhone = customer.Phone;
        dto.CustomerAddress = customer.Address;
        dto.CustomerEmail = customer.Email;

        return dto;
    }

    public async Task<List<InvoiceHistoryDto>> GetAllAsync()
    {
        var invoices = await _invoiceRepository.GetAllAsync();
        var result = new List<InvoiceHistoryDto>();

        foreach (var invoice in invoices)
        {
            var customer = await _customerRepository.GetByIdAsync(invoice.CustomerId);

            result.Add(new InvoiceHistoryDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                CustomerName = customer is null
                    ? "Cliente no encontrado"
                    : $"{customer.FirstName} {customer.LastName}",
                Date = invoice.Date,
                Subtotal = invoice.Subtotal,
                Tax = invoice.Tax,
                Total = invoice.Total
            });
        }

        return result;
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id);

        if (invoice is null)
            return null;

        var customer = await _customerRepository.GetByIdAsync(invoice.CustomerId);

        var dto = MapInvoiceToDto(invoice);

        dto.CustomerName = customer is null
            ? "Cliente no encontrado"
            : $"{customer.FirstName} {customer.LastName}";

        dto.CustomerPhone = customer?.Phone ?? "No registrado";
        dto.CustomerAddress = customer?.Address ?? "No registrado";
        dto.CustomerEmail = customer?.Email ?? "No registrado";

        return dto;
    }

    private static InvoiceDto MapInvoiceToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerId = invoice.CustomerId,
            Date = invoice.Date,
            Subtotal = invoice.Subtotal,
            Tax = invoice.Tax,
            Total = invoice.Total,
            Details = invoice.Details.Select(item => new InvoiceDetailDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity,
                Subtotal = item.Subtotal
            }).ToList()
        };
    }
}