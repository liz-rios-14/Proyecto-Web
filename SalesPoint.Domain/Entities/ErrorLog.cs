using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public sealed class ErrorLog : BaseEntity
{
    public string Source { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Detail { get; private set; } = string.Empty;
    public string? StackTrace { get; private set; }
    public string ExceptionType { get; private set; } = string.Empty;
    public int? UserId { get; private set; }
    public string HttpMethod { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime Date => CreatedAt;

    private ErrorLog() { }

    public ErrorLog(
        string source,
        string message,
        string? detail = null,
        string? exceptionType = null,
        int? userId = null,
        string? httpMethod = null,
        string? path = null)
    {
        SetSource(source);
        SetMessage(message);
        SetDetail(detail);
        SetStackTrace(detail);
        ExceptionType = Limit(exceptionType, 200);
        UserId = userId;
        HttpMethod = Limit(httpMethod, 10).ToUpperInvariant();
        Path = Limit(path, 300);
        CreatedAt = DateTime.UtcNow;
    }

    public void SetSource(string source)
    {
        var cleanSource = source?.Trim() ?? "UNKNOWN";

        if (string.IsNullOrWhiteSpace(cleanSource))
            cleanSource = "UNKNOWN";

        Source = cleanSource.Length > 120
            ? cleanSource[..120]
            : cleanSource;
    }

    public void SetMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("El mensaje del error es obligatorio.");

        var cleanMessage = message.Trim();
        Message = cleanMessage.Length > 1000
            ? cleanMessage[..1000]
            : cleanMessage;
    }

    public void SetDetail(string? detail)
    {
        Detail = detail?.Trim() ?? string.Empty;
    }

    public void SetStackTrace(string? stackTrace)
    {
        StackTrace = stackTrace?.Trim();
    }

    private static string Limit(string? value, int maxLength)
    {
        var cleanValue = value?.Trim() ?? string.Empty;
        return cleanValue.Length > maxLength
            ? cleanValue[..maxLength]
            : cleanValue;
    }
}
