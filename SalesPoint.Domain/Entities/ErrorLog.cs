using SalesPoint.Domain.Common;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Domain.Entities;

public class ErrorLog : BaseEntity
{
    public string Message { get; private set; } = string.Empty;
    public string StackTrace { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public DateTime Date { get; private set; } = DateTime.UtcNow;

    private ErrorLog()
    {
    }

    public ErrorLog(
        string message,
        string stackTrace,
        string source)
    {
        SetMessage(message);
        SetStackTrace(stackTrace);
        SetSource(source);
        Date = DateTime.UtcNow;
    }

    public void SetMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("El mensaje del error es obligatorio.");

        Message = message.Trim();
    }

    public void SetStackTrace(string stackTrace)
    {
        StackTrace = stackTrace?.Trim() ?? string.Empty;
    }

    public void SetSource(string source)
    {
        var cleanSource = source?.Trim() ?? string.Empty;

        if (cleanSource.Length > 200)
            throw new DomainException("El origen del error no puede superar los 200 caracteres.");

        Source = cleanSource;
    }
}
