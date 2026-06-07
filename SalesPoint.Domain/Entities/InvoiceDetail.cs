using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public class InvoiceDetail
{
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    public decimal Subtotal => decimal.Round(Price * Quantity, 2);

    private InvoiceDetail()
    {
    }

    public InvoiceDetail(int productId, string productName, decimal price, int quantity)
    {
        if (productId <= 0)
            throw new DomainException("El producto es obligatorio.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("El nombre del producto es obligatorio.");

        if (price <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        ProductId = productId;
        ProductName = productName.Trim().ToUpperInvariant();
        Price = decimal.Round(price, 2);
        Quantity = quantity;
    }
}