using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class SystemStatusRepository : ISystemStatusRepository
{
    private readonly AppDbContext _context;

    public SystemStatusRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> CanConnectToDatabaseAsync() =>
        _context.Database.CanConnectAsync();
}
