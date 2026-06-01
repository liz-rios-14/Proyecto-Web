using SalesPoint.Domain.Common;
using SalesPoint.Domain.Enums;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class StockMovement : BaseEntity
{
    public int ProductId { get; private set; }
    public int? SaleId { get; private set; }
    public int? InvoiceId { get; private set; }

    public string MovementType { get; private set; } = string.Empty;
    public StockMovementType Type { get; private set; }

    public int Quantity { get; private set; }
    public int StockAfter { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime Date => CreatedAt;

    public string Reason { get; private set; } = string.Empty;
    public string Observation => Reason;

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
        SetProductId(productId);
        SetQuantityAllowNegative(quantity);
        SetMovementType(movementType);
        StockAfter = stockAfter;
        Reason = reason?.Trim().ToUpperInvariant() ?? string.Empty;
        SaleId = saleId;
        InvoiceId = invoiceId;
        CreatedAt = DateTime.UtcNow;
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
        StockAfter = 0;
        CreatedAt = DateTime.UtcNow;
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

    private void SetQuantityAllowNegative(int quantity)
    {
        if (quantity == 0)
            throw new DomainException("La cantidad del movimiento no puede ser cero.");

        Quantity = quantity;
    }

    public void SetType(StockMovementType type)
    {
        Type = type;
        MovementType = type.ToString().ToUpperInvariant();
    }

    public void SetMovementType(string movementType)
    {
        if (string.IsNullOrWhiteSpace(movementType))
            throw new DomainException("El tipo de movimiento es obligatorio.");

        MovementType = movementType.Trim().ToUpperInvariant();
    }

    public void SetObservation(string observation)
    {
        var cleanObservation = observation?.Trim().ToUpperInvariant() ?? string.Empty;

        if (cleanObservation.Length > 300)
            throw new DomainException("La observación del movimiento de stock no puede superar los 300 caracteres.");

        Reason = cleanObservation;
    }
}