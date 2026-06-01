using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(role => role.Id == id);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        var cleanName = name.Trim().ToUpperInvariant();

        return await _context.Roles
            .FirstOrDefaultAsync(role => role.Name == cleanName);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var cleanName = name.Trim().ToUpperInvariant();

        return await _context.Roles
            .AnyAsync(role =>
                role.Name == cleanName &&
                (!excludeId.HasValue || role.Id != excludeId.Value));
    }

    public async Task<Role> CreateAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
    }
}