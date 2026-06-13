using SalesPoint.Application.DTOs.System;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Application.Services;

public sealed class SystemStatusService : ISystemStatusService
{
    private readonly ISystemStatusRepository _repository;

    public SystemStatusService(ISystemStatusRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemStatusDto> GetAsync() =>
        new()
        {
            DatabaseConnected =
                await _repository.CanConnectToDatabaseAsync(),
            CheckedAt = DateTime.UtcNow
        };
}
