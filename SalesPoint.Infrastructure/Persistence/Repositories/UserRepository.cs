using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;
namespace SalesPoint.Infrastructure.Persistence.Repositories;
public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;
    public async Task<List<User>> GetAllAsync() => await _context.Users.Include(u => u.Role).AsNoTracking().OrderBy(u => u.UserName).ToListAsync();
    public async Task<User?> GetByIdAsync(int id) => await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
    public async Task<User?> GetByUserNameOrEmailAsync(string value) { var clean=value.Trim().ToLowerInvariant(); return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserName.ToLower() == clean || u.Email == clean); }
    public async Task<bool> ExistsByUserNameAsync(string userName, int? excludeId = null) { var clean=userName.Trim().ToLowerInvariant(); return await _context.Users.AnyAsync(u => u.UserName.ToLower() == clean && (!excludeId.HasValue || u.Id != excludeId.Value)); }
    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null) { var clean=email.Trim().ToLowerInvariant(); return await _context.Users.AnyAsync(u => u.Email == clean && (!excludeId.HasValue || u.Id != excludeId.Value)); }
    public async Task<User> CreateAsync(User user) { await _context.Users.AddAsync(user); await _context.SaveChangesAsync(); return user; }
    public async Task UpdateAsync(User user) => await _context.SaveChangesAsync();
}
