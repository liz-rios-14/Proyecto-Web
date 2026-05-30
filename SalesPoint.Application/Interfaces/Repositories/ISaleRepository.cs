using SalesPoint.Domain.Entities;
namespace SalesPoint.Application.Interfaces.Repositories;
public interface ISaleRepository
{
    Task<Invoice> CreateAsync(Invoice sale);
    Task UpdateAsync(Invoice sale);
    Task<List<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int id);
}
