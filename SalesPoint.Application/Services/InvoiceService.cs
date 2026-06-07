using SalesPoint.Application.DTOs.Common;
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

        var customerNameSnapshot = string.IsNullOrWhiteSpace(request.CustomerName)
            ? $"{customer.FirstName} {customer.LastName}".Trim()
            : request.CustomerName.Trim();

        var customerEmailSnapshot = string.IsNullOrWhiteSpace(request.CustomerEmail)
            ? customer.Email
            : request.CustomerEmail.Trim();

        var customerPhoneSnapshot = string.IsNullOrWhiteSpace(request.CustomerPhone)
            ? customer.Phone
            : request.CustomerPhone.Trim();

        var customerAddressSnapshot = string.IsNullOrWhiteSpace(request.CustomerAddress)
            ? customer.Address
            : request.CustomerAddress.Trim();

        var invoice = new Invoice(request.CustomerId);

        invoice.SetAuditSnapshot(
            customerName: customerNameSnapshot,
            customerEmail: customerEmailSnapshot,
            customerPhone: customerPhoneSnapshot,
            customerAddress: customerAddressSnapshot,
            sellerId: 1,
            sellerUserName: "admin",
            sellerFullName: "ADMINISTRADOR DEL SISTEMA",
            sellerRole: "ADMINISTRATOR"
        );

        foreach (var detail in request.Details)
        {
            var product = await _productRepository.GetByIdAsync(detail.ProductId);

            if (product is null)
                throw new DomainException("Uno de los productos seleccionados no existe.");

            invoice.AddDetail(product, detail.Quantity);
        }

        invoice.Confirm();

        var createdInvoice = await _invoiceRepository.CreateAsync(invoice);

        var dto = MapInvoiceToDto(createdInvoice);

        dto.CustomerName = createdInvoice.CustomerNameSnapshot;
        dto.CustomerPhone = createdInvoice.CustomerPhoneSnapshot;
        dto.CustomerAddress = createdInvoice.CustomerAddressSnapshot;
        dto.CustomerEmail = createdInvoice.CustomerEmailSnapshot;

        return dto;
    }

    public async Task<PagedResponse<InvoiceHistoryDto>> GetAllAsync(
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        var invoices = await _invoiceRepository.GetAllAsync(pageNumber, pageSize);
        var totalItems = await _invoiceRepository.CountAsync();

        var result = invoices.Select(invoice => new InvoiceHistoryDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerId = invoice.CustomerId,
            CustomerName = string.IsNullOrWhiteSpace(invoice.CustomerNameSnapshot)
                ? "Cliente no registrado"
                : invoice.CustomerNameSnapshot,
            Date = invoice.Date,
            Subtotal = invoice.Subtotal,
            Tax = invoice.Tax,
            Total = invoice.Total
        }).ToList();

        return new PagedResponse<InvoiceHistoryDto>
        {
            Items = result,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id);

        if (invoice is null)
            return null;

        var customer = await _customerRepository.GetByIdAsync(invoice.CustomerId);

        var dto = MapInvoiceToDto(invoice);

        dto.CustomerName = string.IsNullOrWhiteSpace(invoice.CustomerNameSnapshot)
            ? customer is null
                ? "Cliente no encontrado"
                : $"{customer.FirstName} {customer.LastName}".Trim()
            : invoice.CustomerNameSnapshot;

        dto.CustomerPhone = string.IsNullOrWhiteSpace(invoice.CustomerPhoneSnapshot)
            ? customer?.Phone ?? "No registrado"
            : invoice.CustomerPhoneSnapshot;

        dto.CustomerAddress = string.IsNullOrWhiteSpace(invoice.CustomerAddressSnapshot)
            ? customer?.Address ?? "No registrado"
            : invoice.CustomerAddressSnapshot;

        dto.CustomerEmail = string.IsNullOrWhiteSpace(invoice.CustomerEmailSnapshot)
            ? customer?.Email ?? "No registrado"
            : invoice.CustomerEmailSnapshot;

        return dto;
    }

    public async Task<ReconstructedInvoiceDto?> ReconstructByInvoiceNumberAsync(
        string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new DomainException("El número de factura es obligatorio.");

        var invoice = await _invoiceRepository
            .GetByInvoiceNumberForAuditAsync(invoiceNumber.Trim());

        if (invoice is null)
            return null;

        var customer = await _customerRepository.GetByIdAsync(invoice.CustomerId);

        var customerName = string.IsNullOrWhiteSpace(invoice.CustomerNameSnapshot)
            ? customer is null
                ? "Cliente no registrado"
                : $"{customer.FirstName} {customer.LastName}".Trim()
            : invoice.CustomerNameSnapshot;

        var separatedName = SplitCustomerName(customerName);

        return new ReconstructedInvoiceDto
        {
            InvoiceNumber = invoice.InvoiceNumber,
            Date = invoice.Date,

            Customer = new ReconstructedCustomerDto
            {
                CustomerId = invoice.CustomerId,
                Name = customerName,
                FirstName = separatedName.FirstName,
                LastName = separatedName.LastName,

                Email = string.IsNullOrWhiteSpace(invoice.CustomerEmailSnapshot)
                    ? customer?.Email ?? string.Empty
                    : invoice.CustomerEmailSnapshot,

                Phone = string.IsNullOrWhiteSpace(invoice.CustomerPhoneSnapshot)
                    ? customer?.Phone ?? string.Empty
                    : invoice.CustomerPhoneSnapshot,

                Address = string.IsNullOrWhiteSpace(invoice.CustomerAddressSnapshot)
                    ? customer?.Address ?? string.Empty
                    : invoice.CustomerAddressSnapshot
            },

            Seller = new ReconstructedSellerDto
            {
                SellerId = invoice.SellerId,
                UserName = string.IsNullOrWhiteSpace(invoice.SellerUserNameSnapshot)
                    ? "admin"
                    : invoice.SellerUserNameSnapshot,
                FullName = string.IsNullOrWhiteSpace(invoice.SellerFullNameSnapshot)
                    ? "ADMINISTRADOR DEL SISTEMA"
                    : invoice.SellerFullNameSnapshot,
                Role = string.IsNullOrWhiteSpace(invoice.SellerRoleSnapshot)
                    ? "ADMINISTRATOR"
                    : invoice.SellerRoleSnapshot
            },

            Details = invoice.Details.Select(detail => new ReconstructedInvoiceDetailDto
            {
                ProductId = detail.ProductId,
                ProductName = detail.ProductName,
                Quantity = detail.Quantity,
                UnitPrice = detail.Price,
                Subtotal = detail.Subtotal
            }).ToList(),

            Subtotal = invoice.Subtotal,
            Tax = invoice.Tax,
            Total = invoice.Total
        };
    }

    private static (string FirstName, string LastName) SplitCustomerName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (string.Empty, string.Empty);

        var parts = fullName
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
            return (parts[0], string.Empty);

        if (parts.Length == 2)
            return (parts[0], parts[1]);

        return (
            string.Join(" ", parts.Take(2)),
            string.Join(" ", parts.Skip(2))
        );
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
            CustomerName = invoice.CustomerNameSnapshot,
            CustomerPhone = invoice.CustomerPhoneSnapshot,
            CustomerAddress = invoice.CustomerAddressSnapshot,
            CustomerEmail = invoice.CustomerEmailSnapshot,
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