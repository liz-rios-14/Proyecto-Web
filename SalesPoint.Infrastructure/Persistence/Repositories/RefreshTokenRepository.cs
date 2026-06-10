using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        return _context.RefreshTokens
            .Include(item => item.User)
            .ThenInclude(user => user!.Role)
            .FirstOrDefaultAsync(item => item.TokenHash == tokenHash);
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();

    public async Task RevokeAllActiveAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(item => item.UserId == userId && item.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.Revoke();

        await _context.SaveChangesAsync();
    }
}
