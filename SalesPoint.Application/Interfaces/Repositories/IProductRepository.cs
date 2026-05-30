using SalesPoint.Domain.Entities;
namespace SalesPoint.Application.Interfaces.Repositories;
public interface IProductRepository
{
    Task<List<Product>> SearchAsync(string field, string value);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}
