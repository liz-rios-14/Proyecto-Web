using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class ErrorLogRepository : IErrorLogRepository
{
    private readonly AppDbContext _context;

    public ErrorLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorLog> CreateAsync(ErrorLog errorLog)
    {
        await _context.ErrorLogs.AddAsync(errorLog);
        await _context.SaveChangesAsync();
        return errorLog;
    }

    public async Task<List<ErrorLog>> GetAllAsync()
    {
        return await _context.ErrorLogs
            .AsNoTracking()
            .OrderByDescending(error => error.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ErrorLog>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize)
    {
        var query = BuildQuery(field, value);

        return await query
            .OrderByDescending(error => error.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string field, string value)
    {
        return await BuildQuery(field, value).CountAsync();
    }

    private IQueryable<ErrorLog> BuildQuery(string field, string value)
    {
        var query = _context.ErrorLogs.AsNoTracking();
        var cleanField = field?.Trim().ToLowerInvariant() ?? string.Empty;
        var cleanValue = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cleanValue))
            return query;

        if (cleanField == "id" && int.TryParse(cleanValue, out var id))
            return query.Where(error => error.Id == id);

        if (cleanField == "userid" && int.TryParse(cleanValue, out var userId))
            return query.Where(error => error.UserId == userId);

        if (cleanField == "source")
            return query.Where(error => error.Source.Contains(cleanValue));

        if (cleanField == "message")
            return query.Where(error => error.Message.Contains(cleanValue));

        if (cleanField == "exceptiontype")
            return query.Where(error => error.ExceptionType.Contains(cleanValue));

        if (cleanField == "path")
            return query.Where(error => error.Path.Contains(cleanValue));

        if (cleanField == "httpmethod")
            return query.Where(error => error.HttpMethod.Contains(cleanValue));

        return query.Where(error =>
            error.Source.Contains(cleanValue) ||
            error.Message.Contains(cleanValue) ||
            error.ExceptionType.Contains(cleanValue) ||
            error.Path.Contains(cleanValue) ||
            error.HttpMethod.Contains(cleanValue));
    }
}
