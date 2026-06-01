using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class StockMovement : BaseEntity
{
    public int ProductId { get; private set; }
    public int? SaleId { get; private set; }
    public int? InvoiceId { get; private set; }
    public string MovementType { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public int StockAfter { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    public Product? Product { get; private set; }
    public Sale? Sale { get; private set; }

    private StockMovement() { }

    public StockMovement(
        int productId,
        string movementType,
        int quantity,
        int stockAfter = 0,
        string reason = "",
        int? saleId = null,
        int? invoiceId = null)
    {
        if (productId <= 0)
            throw new DomainException("El producto del movimiento de stock es obligatorio.");

        if (string.IsNullOrWhiteSpace(movementType))
            throw new DomainException("El tipo de movimiento es obligatorio.");

        if (quantity == 0)
            throw new DomainException("La cantidad del movimiento no puede ser cero.");

        ProductId = productId;
        MovementType = movementType.Trim().ToUpperInvariant();
        Quantity = quantity;
        StockAfter = stockAfter;
        Reason = reason?.Trim().ToUpperInvariant() ?? string.Empty;
        SaleId = saleId;
        InvoiceId = invoiceId;
        CreatedAt = DateTime.UtcNow;
    }
}