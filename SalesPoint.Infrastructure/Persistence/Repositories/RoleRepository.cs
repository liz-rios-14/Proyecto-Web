using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context) => _context = context;

    public async Task<Role?> GetByIdAsync(int id) => await _context.Roles.FirstOrDefaultAsync(role => role.Id == id);

    public async Task<Role?> GetByNameAsync(string name)
    {
        var cleanName = name.Trim().ToUpperInvariant();
        return await _context.Roles.FirstOrDefaultAsync(role => role.Name == cleanName);
    }

    public async Task<List<Role>> GetAllAsync() => await _context.Roles.OrderBy(role => role.Id).ToListAsync();

    public async Task<Role> CreateAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
        return role;
    }
}
