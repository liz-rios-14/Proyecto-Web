using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<Role> CreateAsync(Role role);
    Task UpdateAsync(Role role);
}