using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<List<Product>> SearchAsync(string field, string value);
    Task<List<Product>> SearchAsync(string field, string value, int pageNumber, int pageSize);
    Task<int> CountAsync(string field, string value);
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}