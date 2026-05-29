using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<List<Customer>> SearchAsync(string field, string value);
    Task<List<Customer>> SearchAsync(string field, string value, int pageNumber, int pageSize);
    Task<int> CountAsync(string field, string value);
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
}