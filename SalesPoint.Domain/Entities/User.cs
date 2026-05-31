using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SalesPoint.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public int RoleId { get; private set; }

    private User()
    {
    }

    public User(
        string username,
        string email,
        string passwordHash,
        int roleId)
    {
        SetUsername(username);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetRoleId(roleId);
    }

    public void SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("El nombre de usuario es obligatorio.");

        Username = username.Trim().ToUpperInvariant();
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo del usuario es obligatorio.");

        var cleanEmail = email.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(
                cleanEmail,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new DomainException("El correo del usuario no es válido.");
        }

        Email = cleanEmail;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("La contraseña del usuario es obligatoria.");

        PasswordHash = passwordHash.Trim();
    }

    public void SetRoleId(int roleId)
    {
        if (roleId <= 0)
            throw new DomainException("El rol del usuario es obligatorio.");

        RoleId = roleId;
    }

    public void Update(
        string username,
        string email,
        string passwordHash,
        int roleId)
    {
        SetUsername(username);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetRoleId(roleId);
    }
}
