using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

// Refresh Token
public sealed class RefreshToken : BaseEntity
{
    public int UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public User? User { get; private set; }

    public bool IsValid => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    private RefreshToken() { }

    public RefreshToken(int userId, string tokenHash, DateTime expiresAt)
    {
        if (userId <= 0)
            throw new DomainException("El usuario del token es obligatorio.");
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("El token de renovación es obligatorio.");

        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        if (RevokedAt is not null)
            return;

        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
