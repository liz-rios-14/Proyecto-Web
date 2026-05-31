using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.Include(user => user.Role).FirstOrDefaultAsync(user => user.Id == id);

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        var cleanUserName = userName.Trim().ToLowerInvariant();
        return await _context.Users.Include(user => user.Role).FirstOrDefaultAsync(user => user.UserName == cleanUserName);
    }

    public async Task<List<User>> GetAllAsync() =>
        await _context.Users.Include(user => user.Role).OrderBy(user => user.Id).ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user) => await _context.SaveChangesAsync();
}
