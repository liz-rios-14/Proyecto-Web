using SalesPoint.Domain.Common;

namespace SalesPoint.Domain.Entities;

public sealed class PasswordHistory : BaseEntity
{
    public int UserId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;

    public User? User { get; private set; }

    private PasswordHistory() { }

    public PasswordHistory(int userId, string passwordHash)
    {
        UserId = userId;
        PasswordHash = passwordHash;
    }
}