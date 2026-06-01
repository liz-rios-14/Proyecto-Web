using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class PaymentMethod : BaseEntity
{
    private readonly List<Sale> _sales = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<Sale> Sales => _sales.AsReadOnly();

    private PaymentMethod() { }

    public PaymentMethod(string name)
    {
        SetName(name);
        Description = string.Empty;
        IsActive = true;
    }

    public PaymentMethod(string name, string description)
    {
        SetName(name);
        SetDescription(description);
        IsActive = true;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del método de pago es obligatorio.");

        var cleanName = name.Trim().ToUpperInvariant();

        if (cleanName.Length < 2)
            throw new DomainException("El nombre del método de pago debe tener al menos 2 caracteres.");

        if (cleanName.Length > 50)
            throw new DomainException("El nombre del método de pago no puede superar los 50 caracteres.");

        Name = cleanName;
    }

    public void SetDescription(string description)
    {
        var cleanDescription = description?.Trim().ToUpperInvariant() ?? string.Empty;

        if (cleanDescription.Length > 200)
            throw new DomainException("La descripción del método de pago no puede superar los 200 caracteres.");

        Description = cleanDescription;
    }

    public void Update(string name, string description)
    {
        SetName(name);
        SetDescription(description);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}