using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(user => user.Role)
            .AsNoTracking()
            .OrderBy(user => user.UserName)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        var cleanUserName = userName.Trim().ToLowerInvariant();

        return await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.UserName == cleanUserName);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var cleanEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Email == cleanEmail);
    }

    public async Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail)
    {
        var cleanValue = userNameOrEmail.Trim().ToLowerInvariant();

        return await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user =>
                user.UserName == cleanValue ||
                user.Email == cleanValue);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, int? excludeId = null)
    {
        var cleanUserName = userName.Trim().ToLowerInvariant();

        return await _context.Users
            .AnyAsync(user =>
                user.UserName == cleanUserName &&
                (!excludeId.HasValue || user.Id != excludeId.Value));
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        var cleanEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .AnyAsync(user =>
                user.Email == cleanEmail &&
                (!excludeId.HasValue || user.Id != excludeId.Value));
    }

    public async Task<List<string>> GetPasswordHistoryHashesAsync(int userId)
    {
        return await _context.PasswordHistories
            .Where(history => history.UserId == userId)
            .OrderByDescending(history => history.CreatedAt)
            .Select(history => history.PasswordHash)
            .ToListAsync();
    }

    public async Task AddPasswordHistoryAsync(PasswordHistory history)
    {
        await _context.PasswordHistories.AddAsync(history);
        await _context.SaveChangesAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}