using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<List<Invoice>> GetAllAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Invoice?> GetByIdAsync(int id);
}