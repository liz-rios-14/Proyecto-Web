using SalesPoint.Domain.Common;
using SalesPoint.Domain.Enums;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public StockMovementType Type { get; private set; }
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    public string Observation { get; private set; } = string.Empty;

    private StockMovement()
    {
    }

    public StockMovement(
        int productId,
        int quantity,
        StockMovementType type,
        string observation)
    {
        SetProductId(productId);
        SetQuantity(quantity);
        SetType(type);
        SetObservation(observation);
        Date = DateTime.UtcNow;
    }

    public void SetProductId(int productId)
    {
        if (productId <= 0)
            throw new DomainException("El producto del movimiento de stock es obligatorio.");

        ProductId = productId;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad del movimiento de stock debe ser mayor a cero.");

        Quantity = quantity;
    }

    public void SetType(StockMovementType type)
    {
        Type = type;
    }

    public void SetObservation(string observation)
    {
        var cleanObservation = observation?.Trim().ToUpperInvariant() ?? string.Empty;

        if (cleanObservation.Length > 300)
            throw new DomainException("La observación del movimiento de stock no puede superar los 300 caracteres.");

        Observation = cleanObservation;
    }
}
