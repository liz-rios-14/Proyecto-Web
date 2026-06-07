using SalesPoint.Domain.Entities;
namespace SalesPoint.Application.Interfaces.Repositories;
public interface ICustomerRepository
{
    Task<List<Customer>> SearchAsync(string field, string value, bool onlyActive);
    Task<List<Customer>> SearchAsync(string field, string value, int pageNumber, int pageSize, bool onlyActive);
    Task<int> CountAsync(string field, string value, bool onlyActive);
    Task<Customer?> GetByIdAsync(int id);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    Task<bool> ExistsByCedulaAsync(string cedula, int? excludeId = null);
    Task<bool> HasHistoryAsync(int id);
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
}
