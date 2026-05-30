using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    private Role() { }
    public Role(string name, string description = "") { Update(name, description); IsActive = true; }
    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("El rol es obligatorio.");
        Name = name.Trim().ToUpperInvariant();
        Description = description?.Trim() ?? string.Empty;
    }
    public void Deactivate() => IsActive = false;
}
