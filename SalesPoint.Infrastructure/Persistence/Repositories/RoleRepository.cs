using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;
namespace SalesPoint.Infrastructure.Persistence.Repositories;
public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;
    public RoleRepository(AppDbContext context) => _context = context;
    public async Task<List<Role>> GetAllAsync() => await _context.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
    public async Task<Role?> GetByIdAsync(int id) => await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
    public async Task<Role?> GetByNameAsync(string name) => await _context.Roles.FirstOrDefaultAsync(r => r.Name == name.Trim().ToUpperInvariant());
    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null) { var clean=name.Trim().ToUpperInvariant(); return await _context.Roles.AnyAsync(r => r.Name == clean && (!excludeId.HasValue || r.Id != excludeId.Value)); }
    public async Task<Role> CreateAsync(Role role) { await _context.Roles.AddAsync(role); await _context.SaveChangesAsync(); return role; }
    public async Task UpdateAsync(Role role) => await _context.SaveChangesAsync();
}
