namespace SalesPoint.Application.DTOs.ErrorLogs;

public sealed class ErrorLogDto
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class CreateErrorLogRequest
{
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public sealed class ErrorLogQuery
{
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class RegisterErrorLogRequest
{
    public Exception Exception { get; set; } = new Exception();
    public string Source { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}
