using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SalesPoint.Domain.Entities;

public sealed class User : BaseEntity
{
    public int RoleId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Username => UserName;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int FailedLoginAttempts { get; private set; }
    public bool IsLocked { get; private set; }

    public string? PasswordResetTokenHash { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public Role? Role { get; private set; }

    private User() { }

    public User(int roleId, string fullName, string userName, string email, string passwordHash)
    {
        SetRole(roleId);
        SetFullName(fullName);
        SetUserName(userName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        IsActive = true;
        FailedLoginAttempts = 0;
        IsLocked = false;
    }

    public User(string userName, string email, string passwordHash, int roleId)
        : this(roleId, userName, userName, email, passwordHash)
    {
    }

    public void SetRole(int roleId)
    {
        if (roleId <= 0)
            throw new DomainException("El rol del usuario es obligatorio.");

        RoleId = roleId;
    }

    public void SetRoleId(int roleId)
    {
        SetRole(roleId);
    }

    public void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("El nombre del usuario es obligatorio.");

        var cleanFullName = Regex.Replace(fullName.Trim(), @"\s+", " ");

        if (!Regex.IsMatch(cleanFullName, @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+(?: [A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+)*$"))
            throw new DomainException("Los nombres y apellidos solo pueden contener letras y espacios simples.");

        FullName = cleanFullName.ToUpperInvariant();
    }

    public void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("El nombre de usuario es obligatorio.");

        var cleanUserName = userName.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(cleanUserName, @"^[a-z0-9._-]+$"))
            throw new DomainException("El usuario no puede contener espacios.");

        UserName = cleanUserName;
    }

    public void SetUsername(string userName)
    {
        SetUserName(userName);
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

    public void ChangePassword(string newPasswordHash)
    {
        SetPasswordHash(newPasswordHash);
    }

    public void SetPasswordResetToken(string tokenHash, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("El token de recuperación es obligatorio.");

        PasswordResetTokenHash = tokenHash.Trim();
        PasswordResetTokenExpiresAt = expiresAt;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetTokenHash = null;
        PasswordResetTokenExpiresAt = null;
    }

    public void Update(
        string fullName,
        string userName,
        string email,
        int roleId,
        bool isActive)
    {
        SetUserName(userName);
        SetFullName(fullName);
        SetEmail(email);
        SetRole(roleId);
        IsActive = isActive;
    }

    public void Update(string userName, string email, string passwordHash, int roleId)
    {
        SetUserName(userName);
        SetFullName(userName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetRole(roleId);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Disable()
    {
        Deactivate();
    }

    public void RegisterFailedLoginAttempt()
    {
        if (IsLocked)
            return;

        FailedLoginAttempts++;

        if (FailedLoginAttempts >= 3)
            IsLocked = true;
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
    }

    public void Unlock()
    {
        IsLocked = false;
        FailedLoginAttempts = 0;
    }
}
