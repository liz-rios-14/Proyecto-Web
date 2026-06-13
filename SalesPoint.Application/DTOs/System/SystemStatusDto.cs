namespace SalesPoint.Application.DTOs.System;

public sealed class SystemStatusDto
{
    public bool DatabaseConnected { get; set; }
    public DateTime CheckedAt { get; set; }
}
