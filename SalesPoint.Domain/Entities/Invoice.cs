using SalesPoint.Domain.Common;
using SalesPoint.Domain.Enums;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public class Invoice : BaseEntity
{
    private readonly List<InvoiceDetail> _details = new();
    private readonly Dictionary<int, Product> _products = new();

    public int CustomerId { get; private set; }
    public DateTime Date { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public SaleStatus Status { get; private set; } = SaleStatus.Draft;

    public string CustomerNameSnapshot { get; private set; } = string.Empty;
    public string CustomerCedulaSnapshot { get; private set; } = string.Empty;
    public string CustomerEmailSnapshot { get; private set; } = string.Empty;
    public string CustomerPhoneSnapshot { get; private set; } = string.Empty;
    public string CustomerAddressSnapshot { get; private set; } = string.Empty;

    public int SellerId { get; private set; }
    public string SellerUserNameSnapshot { get; private set; } = string.Empty;
    public string SellerFullNameSnapshot { get; private set; } = string.Empty;
    public string SellerRoleSnapshot { get; private set; } = string.Empty;

    public IReadOnlyCollection<InvoiceDetail> Details => _details.AsReadOnly();

    public decimal Subtotal => _details.Sum(detail => detail.Subtotal);
    public decimal Tax => decimal.Round(Subtotal * 0.12m, 2);
    public decimal Total => Subtotal + Tax;

    private Invoice() { }

    public Invoice(int customerId)
    {
        if (customerId <= 0)
            throw new DomainException("El cliente es obligatorio.");

        CustomerId = customerId;
        Date = DateTime.UtcNow;
        Status = SaleStatus.Draft;
    }

    public void AssignInvoiceNumber(int sequence)
    {
        if (sequence <= 0)
            throw new DomainException("La secuencia de factura no es válida.");

        InvoiceNumber = $"FAC-{sequence:D10}";
    }

    public void SetAuditSnapshot(
        string customerName,
        string customerCedula,
        string customerEmail,
        string customerPhone,
        string customerAddress,
        int sellerId,
        string sellerUserName,
        string sellerFullName,
        string sellerRole)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("El nombre del cliente para auditoría es obligatorio.");

        if (sellerId <= 0)
            throw new DomainException("El vendedor para auditoría es obligatorio.");

        if (string.IsNullOrWhiteSpace(sellerUserName))
            throw new DomainException("El usuario vendedor para auditoría es obligatorio.");

        CustomerNameSnapshot = customerName.Trim().ToUpperInvariant();
        CustomerCedulaSnapshot = customerCedula?.Trim() ?? string.Empty;
        CustomerEmailSnapshot = customerEmail?.Trim().ToLowerInvariant() ?? string.Empty;
        CustomerPhoneSnapshot = customerPhone?.Trim() ?? string.Empty;
        CustomerAddressSnapshot = customerAddress?.Trim().ToUpperInvariant() ?? string.Empty;

        SellerId = sellerId;
        SellerUserNameSnapshot = sellerUserName.Trim().ToLowerInvariant();

        SellerFullNameSnapshot = string.IsNullOrWhiteSpace(sellerFullName)
            ? sellerUserName.Trim().ToUpperInvariant()
            : sellerFullName.Trim().ToUpperInvariant();

        SellerRoleSnapshot = string.IsNullOrWhiteSpace(sellerRole)
            ? "SELLER"
            : sellerRole.Trim().ToUpperInvariant();
    }

    public void AddDetail(Product product, int quantity)
    {
        if (product is null)
            throw new DomainException("El producto es obligatorio.");

        if (_details.Any(detail => detail.ProductId == product.Id))
            throw new DuplicateProductException("El producto ya fue agregado a la factura.");

        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        var detail = new InvoiceDetail(
            product.Id,
            product.Name,
            product.Price,
            quantity
        );

        _details.Add(detail);
        _products[product.Id] = product;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(InvoiceNumber))
            throw new DomainException("El número de factura es obligatorio.");

        ValidateCustomer();
        ValidateSellerSnapshot();
        ValidateDetails();
    }

    public void Confirm()
    {
        ValidateCustomer();
        ValidateSellerSnapshot();
        ValidateDetails();

        if (Status != SaleStatus.Draft)
            throw new InvalidSaleStateException("Solo una venta en borrador puede confirmarse.");

        ValidateStock();
        DecreaseStock();

        Status = SaleStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status != SaleStatus.Confirmed)
            throw new InvalidSaleStateException("Solo una venta confirmada puede cancelarse.");

        Status = SaleStatus.Cancelled;
    }

    public void Delete()
    {
        if (Status == SaleStatus.Confirmed)
            throw new InvalidSaleStateException("No se puede eliminar una venta confirmada.");

        IsDeleted = true;
    }

    private void ValidateCustomer()
    {
        if (CustomerId <= 0)
            throw new DomainException("El cliente es obligatorio.");
    }

    private void ValidateSellerSnapshot()
    {
        if (string.IsNullOrWhiteSpace(CustomerNameSnapshot))
            throw new DomainException("Los datos históricos del cliente son obligatorios.");

        if (SellerId <= 0)
            throw new DomainException("Los datos históricos del vendedor son obligatorios.");

        if (string.IsNullOrWhiteSpace(SellerUserNameSnapshot))
            throw new DomainException("El usuario histórico del vendedor es obligatorio.");
    }

    private void ValidateDetails()
    {
        if (!_details.Any())
            throw new DomainException("La factura debe tener al menos un producto.");
    }

    private void ValidateStock()
    {
        foreach (var detail in _details)
        {
            if (!_products.TryGetValue(detail.ProductId, out var product))
                throw new DomainException("No se pudo validar el stock del producto.");

            if (detail.Quantity > product.Stock)
                throw new InsufficientStockException("No existe stock suficiente.");
        }
    }

    private void DecreaseStock()
    {
        foreach (var detail in _details)
        {
            var product = _products[detail.ProductId];
            product.DecreaseStock(detail.Quantity);
        }
    }
}
