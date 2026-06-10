using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task AddAsync(RefreshToken token);
    Task SaveChangesAsync();
    Task RevokeAllActiveAsync(int userId);
}
