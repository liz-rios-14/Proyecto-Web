namespace SalesPoint.Application.DTOs.Common;

public sealed class DeleteResultDto
{
    public bool WasPhysical { get; set; }
    public string Message { get; set; } = string.Empty;
}
