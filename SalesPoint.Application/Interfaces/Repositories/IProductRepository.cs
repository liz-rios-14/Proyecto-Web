using SalesPoint.Domain.Entities;
namespace SalesPoint.Application.Interfaces.Repositories;
public interface IProductRepository
{
    Task<List<Product>> SearchAsync(string field, string value, bool onlyAvailable);
    Task<List<Product>> SearchAsync(string field, string value, int pageNumber, int pageSize, bool onlyAvailable);
    Task<int> CountAsync(string field, string value, bool onlyAvailable);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> HasHistoryAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}
