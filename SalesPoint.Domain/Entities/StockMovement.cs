using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; private set; }
    public Product? Product { get; private set; }
    public int Quantity { get; private set; }
    public string MovementType { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    private StockMovement() { }
    public StockMovement(int productId, int quantity, string movementType, string reason)
    {
        if (productId <= 0) throw new DomainException("El producto es obligatorio.");
        if (quantity <= 0) throw new DomainException("La cantidad debe ser mayor a cero.");
        ProductId = productId; Quantity = quantity; MovementType = movementType.Trim(); Reason = reason?.Trim() ?? string.Empty; CreatedAt = DateTime.UtcNow;
    }
}
