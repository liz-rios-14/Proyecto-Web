using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;
namespace SalesPoint.Infrastructure.Persistence.Repositories;
public sealed class ErrorLogRepository : IErrorLogRepository
{
    private readonly AppDbContext _context;
    public ErrorLogRepository(AppDbContext context) => _context = context;
    public async Task<List<ErrorLog>> GetAllAsync() => await _context.ErrorLogs.AsNoTracking().OrderByDescending(e => e.CreatedAt).ToListAsync();
    public async Task<ErrorLog> CreateAsync(ErrorLog errorLog) { await _context.ErrorLogs.AddAsync(errorLog); await _context.SaveChangesAsync(); return errorLog; }
}
