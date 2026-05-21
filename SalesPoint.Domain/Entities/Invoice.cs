using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public class Invoice : BaseEntity
{
    private readonly List<InvoiceDetail> _details = new();

    public int CustomerId { get; private set; }
    public DateTime Date { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;

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

        product.DecreaseStock(quantity);

        var detail = new InvoiceDetail(
            product.Id,
            product.Name,
            product.Price,
            quantity
        );

        _details.Add(detail);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(InvoiceNumber))
            throw new DomainException("El número de factura es obligatorio.");

        if (!_details.Any())
            throw new DomainException("La factura debe tener al menos un producto.");
    }
}