using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SalesPoint.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public int RoleId { get; private set; }
    public Role? Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    private User() { }
    public User(string userName, string email, string passwordHash, int roleId)
    {
        SetUserName(userName); SetEmail(email); SetPasswordHash(passwordHash); SetRole(roleId); IsActive = true;
    }
    public void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new DomainException("El usuario es obligatorio.");
        UserName = userName.Trim();
    }
    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("El correo es obligatorio.");
        var clean = email.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(clean, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new DomainException("El correo no es válido.");
        Email = clean;
    }
    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("La contraseña es obligatoria.");
        PasswordHash = passwordHash;
    }
    public void SetRole(int roleId)
    {
        if (roleId <= 0) throw new DomainException("El rol es obligatorio.");
        RoleId = roleId;
    }
    public void Update(string userName, string email, int roleId, bool isActive)
    {
        SetUserName(userName); SetEmail(email); SetRole(roleId); IsActive = isActive;
    }
    public void Disable() => IsActive = false;
}
