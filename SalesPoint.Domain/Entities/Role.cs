using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class Role : BaseEntity
{
    private readonly List<User> _users = new();

    public string Name { get; private set; } = string.Empty;
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private Role() { }

    public Role(string name) => SetName(name);

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El rol es obligatorio.");

        var cleanName = name.Trim().ToUpperInvariant();

        if (cleanName.Length > 40)
            throw new DomainException("El rol no puede superar los 40 caracteres.");

        Name = cleanName;
    }
}
