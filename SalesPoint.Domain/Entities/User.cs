using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SalesPoint.Domain.Entities;

public sealed class User : BaseEntity
{
    public int RoleId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public Role? Role { get; private set; }

    private User() { }

    public User(int roleId, string fullName, string userName, string email, string passwordHash)
    {
        SetRole(roleId);
        SetFullName(fullName);
        SetUserName(userName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
    }

    public void SetRole(int roleId)
    {
        if (roleId <= 0)
            throw new DomainException("El rol del usuario es obligatorio.");

        RoleId = roleId;
    }

    public void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("El nombre del usuario es obligatorio.");

        FullName = fullName.Trim().ToUpperInvariant();
    }

    public void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("El nombre de usuario es obligatorio.");

        UserName = userName.Trim().ToLowerInvariant();
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo del usuario es obligatorio.");

        var cleanEmail = email.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(cleanEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException("El correo del usuario no es válido.");

        Email = cleanEmail;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("La contraseña del usuario es obligatoria.");

        PasswordHash = passwordHash.Trim();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
