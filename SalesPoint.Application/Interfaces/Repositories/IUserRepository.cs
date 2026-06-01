using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail);
    Task<bool> ExistsByUserNameAsync(string userName, int? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
}