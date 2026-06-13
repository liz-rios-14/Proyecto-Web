using SalesPoint.Application.DTOs.System;

namespace SalesPoint.Application.Interfaces.Services;

public interface ISystemStatusService
{
    Task<SystemStatusDto> GetAsync();
}
