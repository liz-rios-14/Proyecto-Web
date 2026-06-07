using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SalesPoint.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product() { }

    public Product(string name, decimal price, int stock)
    {
        SetName(name);
        SetPrice(price);
        SetStock(stock);
        IsActive = true;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del producto es obligatorio.");

        var cleanName = Regex.Replace(name.Trim(), @"\s+", " ").ToUpperInvariant();

        if (cleanName.Length < 2)
            throw new DomainException("El nombre del producto debe tener al menos 2 caracteres.");

        if (cleanName.Length > 80)
            throw new DomainException("El nombre del producto no puede superar los 80 caracteres.");

        if (!Regex.IsMatch(cleanName, @"^[A-ZÁÉÍÓÚÜÑ0-9 .,\-/]+$"))
            throw new DomainException("El nombre del producto contiene caracteres no permitidos.");

        Name = cleanName;
    }

    public void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        if (price > 999999.99m)
            throw new DomainException("El precio ingresado es demasiado alto.");

        if (decimal.Round(price, 2) != price)
            throw new DomainException("El precio debe tener máximo dos decimales.");

        Price = price;
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
        return IsActive && Stock > 0;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        if (!IsActive)
            throw new DomainException("El producto está inactivo.");

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

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SoftDelete()
    {
        IsActive = false;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
