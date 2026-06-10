using SalesPoint.Application.DTOs.AuditLogs;
using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

// AuditLog
public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public Task RegisterAsync(RegisterAuditLogRequest request)
    {
        return _repository.AddAsync(new AuditLog(
            request.UserId,
            request.UserName,
            request.Action,
            request.EntityName,
            request.EntityId,
            request.OldValues,
            request.NewValues,
            request.IpAddress,
            request.Path,
            request.HttpMethod));
    }

    public async Task<PagedResponse<AuditLogDto>> SearchAsync(
        AuditLogSearchRequest request)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = new[] { 10, 15, 20, 30 }.Contains(request.PageSize)
            ? request.PageSize
            : 10;

        if (request.StartDate.HasValue && request.EndDate.HasValue &&
            request.EndDate.Value.Date < request.StartDate.Value.Date)
        {
            throw new DomainException(
                "La fecha fin no puede ser menor a la fecha inicio.");
        }

        var logs = await _repository.SearchAsync(request);
        var total = await _repository.CountAsync(request);

        return new PagedResponse<AuditLogDto>
        {
            Items = logs.Select(item => new AuditLogDto
            {
                Id = item.Id,
                UserId = item.UserId,
                UserName = item.UserName,
                Action = item.Action,
                EntityName = item.EntityName,
                EntityId = item.EntityId,
                OldValues = item.OldValues,
                NewValues = item.NewValues,
                CreatedAt = item.CreatedAt,
                IpAddress = item.IpAddress,
                Path = item.Path,
                HttpMethod = item.HttpMethod
            }).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };
    }
}
