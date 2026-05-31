using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class Sale : BaseEntity
{
    private readonly List<SaleDetail> _details = new();

    public int CustomerId { get; private set; }
    public int? UserId { get; private set; }
    public int PaymentMethodId { get; private set; }
    public DateTime Date { get; private set; }
    public string SaleNumber { get; private set; } = string.Empty;
    public bool IsConfirmed { get; private set; }

    public Customer? Customer { get; private set; }
    public User? User { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public IReadOnlyCollection<SaleDetail> Details => _details.AsReadOnly();

    public decimal Subtotal => _details.Sum(x => x.Subtotal);
    public decimal Tax => decimal.Round(Subtotal * 0.12m, 2);
    public decimal Total => Subtotal + Tax;

    private Sale() { }

    public Sale(int customerId, int paymentMethodId, int? userId = null)
    {
        if (customerId <= 0)
            throw new DomainException("El cliente es obligatorio.");

        if (paymentMethodId <= 0)
            throw new DomainException("El método de pago es obligatorio.");

        CustomerId = customerId;
        PaymentMethodId = paymentMethodId;
        UserId = userId;
        Date = DateTime.UtcNow;
    }

    public void AssignSaleNumber(int sequence)
    {
        if (sequence <= 0)
            throw new DomainException("La secuencia de venta no es válida.");

        SaleNumber = $"VEN-{sequence:D10}";
    }

    public void AddDetail(Product product, int quantity)
    {
        if (product is null)
            throw new DomainException("El producto es obligatorio.");

        product.DecreaseStock(quantity);
        _details.Add(new SaleDetail(product.Id, product.Name, product.Price, quantity));
    }

    public void Confirm()
    {
        if (string.IsNullOrWhiteSpace(SaleNumber))
            throw new DomainException("El número de venta es obligatorio.");

        if (!_details.Any())
            throw new DomainException("La venta debe tener al menos un producto.");

        IsConfirmed = true;
    }
}
