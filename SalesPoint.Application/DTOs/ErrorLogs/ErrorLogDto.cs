namespace SalesPoint.Application.DTOs.ErrorLogs;

public sealed class ErrorLogDto
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class CreateErrorLogRequest
{
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}
