using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.DTOs.AuditLogs;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public Task<List<AuditLog>> SearchAsync(AuditLogSearchRequest request)
    {
        return ApplyFilters(request)
            .OrderByDescending(item => item.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
    }

    public Task<int> CountAsync(AuditLogSearchRequest request) =>
        ApplyFilters(request).CountAsync();

    private IQueryable<AuditLog> ApplyFilters(AuditLogSearchRequest request)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.UserName))
            query = query.Where(item => item.UserName.Contains(request.UserName));
        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(item => item.Action.Contains(request.Action));
        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(item => item.EntityName.Contains(request.EntityName));
        if (request.StartDate.HasValue)
            query = query.Where(item => item.CreatedAt >= request.StartDate.Value.Date);
        if (request.EndDate.HasValue)
        {
            var exclusiveEnd = request.EndDate.Value.Date.AddDays(1);
            query = query.Where(item => item.CreatedAt < exclusiveEnd);
        }

        return query;
    }
}
