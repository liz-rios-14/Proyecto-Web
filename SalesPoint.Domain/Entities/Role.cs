using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class Role : BaseEntity
{
    private readonly List<User> _users = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private Role() { }

    public Role(string name, string description = "")
    {
        SetName(name);
        Description = description?.Trim() ?? string.Empty;
        IsActive = true;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El rol es obligatorio.");

        var cleanName = name.Trim().ToUpperInvariant();

        if (cleanName.Length > 40)
            throw new DomainException("El rol no puede superar los 40 caracteres.");

        Name = cleanName;
    }

    public void Update(string name, string description)
    {
        SetName(name);
        Description = description?.Trim() ?? string.Empty;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}