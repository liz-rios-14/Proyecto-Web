using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class ErrorLog : BaseEntity
{
    public string Source { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Detail { get; private set; } = string.Empty;
    public string? StackTrace { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime Date => CreatedAt;

    private ErrorLog() { }

    public ErrorLog(
        string source,
        string message,
        string? detail = null)
    {
        SetSource(source);
        SetMessage(message);
        SetDetail(detail);
        SetStackTrace(detail);
        CreatedAt = DateTime.UtcNow;
    }

    public void SetSource(string source)
    {
        var cleanSource = source?.Trim() ?? "UNKNOWN";

        if (string.IsNullOrWhiteSpace(cleanSource))
            cleanSource = "UNKNOWN";

        if (cleanSource.Length > 200)
            throw new DomainException("El origen del error no puede superar los 200 caracteres.");

        Source = cleanSource;
    }

    public void SetMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("El mensaje del error es obligatorio.");

        Message = message.Trim();
    }

    public void SetDetail(string? detail)
    {
        Detail = detail?.Trim() ?? string.Empty;
    }

    public void SetStackTrace(string? stackTrace)
    {
        StackTrace = stackTrace?.Trim();
    }
}