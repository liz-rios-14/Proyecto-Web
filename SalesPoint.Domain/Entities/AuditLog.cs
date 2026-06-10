using SalesPoint.Domain.Common;

namespace SalesPoint.Domain.Entities;

// AuditLog
public sealed class AuditLog : BaseEntity
{
    public int? UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public string HttpMethod { get; private set; } = string.Empty;

    private AuditLog() { }

    public AuditLog(
        int? userId,
        string userName,
        string action,
        string entityName,
        string? entityId,
        string? oldValues,
        string? newValues,
        string ipAddress,
        string path,
        string httpMethod)
    {
        UserId = userId;
        UserName = userName?.Trim() ?? string.Empty;
        Action = action.Trim();
        EntityName = entityName.Trim();
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress?.Trim() ?? string.Empty;
        Path = path?.Trim() ?? string.Empty;
        HttpMethod = httpMethod?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}
