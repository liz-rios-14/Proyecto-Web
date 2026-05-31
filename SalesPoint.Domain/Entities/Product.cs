using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    private Product()
    {
    }

    public Product(string name, decimal price, int stock)
    {
        SetName(name);
        SetPrice(price);
        SetStock(stock);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del producto es obligatorio.");

        var cleanName = name.Trim().ToUpperInvariant();

        if (cleanName.Length < 2)
            throw new DomainException("El nombre del producto debe tener al menos 2 caracteres.");

        if (cleanName.Length > 80)
            throw new DomainException("El nombre del producto no puede superar los 80 caracteres.");

        Name = cleanName;
    }

    public void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        if (price > 999999.99m)
            throw new DomainException("El precio ingresado es demasiado alto.");

        Price = decimal.Round(price, 2);
    }

    public void SetStock(int stock)
    {
        if (stock < 0)
            throw new DomainException("El stock no puede ser negativo.");

        if (stock > 999999)
            throw new DomainException("El stock ingresado es demasiado alto.");

        Stock = stock;
    }

    public bool HasStock()
    {
        return Stock > 0;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        if (quantity > Stock)
            throw new InsufficientStockException("No existe stock suficiente.");

        Stock -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        Stock += quantity;
    }

    public void Update(string name, decimal price, int stock)
    {
        SetName(name);
        SetPrice(price);
        SetStock(stock);
    }
}
