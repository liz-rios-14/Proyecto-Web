using SalesPoint.Domain.Common;

namespace SalesPoint.Domain.Entities;

public sealed class ErrorLog : BaseEntity
{
    public string Source { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? StackTrace { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ErrorLog() { }

    public ErrorLog(string source, string message, string? stackTrace = null)
    {
        Source = string.IsNullOrWhiteSpace(source) ? "UNKNOWN" : source.Trim();
        Message = string.IsNullOrWhiteSpace(message) ? "Sin mensaje" : message.Trim();
        StackTrace = stackTrace;
        CreatedAt = DateTime.UtcNow;
    }
}
