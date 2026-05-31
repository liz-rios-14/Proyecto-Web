namespace SalesPoint.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; protected set; }
    public bool IsActive { get; protected set; } = true;
    public bool IsDeleted { get; protected set; } = false;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
}
