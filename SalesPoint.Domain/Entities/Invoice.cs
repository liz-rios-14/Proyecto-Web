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

    public IReadOnlyCollection<InvoiceDetail> Details => _details.AsReadOnly();

    public decimal Subtotal => _details.Sum(x => x.Subtotal);
    public decimal Tax => decimal.Round(Subtotal * 0.12m, 2);
    public decimal Total => Subtotal + Tax;

    private Invoice()
    {
    }

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

    public void AddDetail(Product product, int quantity)
    {
        if (product is null)
            throw new DomainException("El producto es obligatorio.");

        if (_details.Any(x => x.ProductId == product.Id))
            throw new DuplicateProductException("El producto ya fue agregado a la factura.");

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

        ValidateDetails();
    }

    public void Confirm()
    {
        ValidateCustomer();
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
