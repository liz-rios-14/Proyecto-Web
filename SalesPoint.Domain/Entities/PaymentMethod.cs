using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class PaymentMethod : BaseEntity
{
    private readonly List<Sale> _sales = new();

    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public IReadOnlyCollection<Sale> Sales => _sales.AsReadOnly();

    private PaymentMethod() { }

    public PaymentMethod(string name) => SetName(name);

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El método de pago es obligatorio.");

        Name = name.Trim().ToUpperInvariant();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
