using SalesPoint.Domain.Common;

namespace SalesPoint.Domain.Entities;

public class ErrorLog : BaseEntity
{
    public string Source { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Detail { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    private ErrorLog() { }
    public ErrorLog(string source, string message, string detail)
    {
        Source = string.IsNullOrWhiteSpace(source) ? "Application" : source.Trim();
        Message = string.IsNullOrWhiteSpace(message) ? "Error sin mensaje" : message.Trim();
        Detail = detail?.Trim() ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
    }
}
