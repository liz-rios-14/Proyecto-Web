using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice> CreateAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
    Task<List<Invoice>> GetAllAsync();
    Task<List<Invoice>> GetAllAsync(int pageNumber, int pageSize);
    Task<int> CountAsync();
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByInvoiceNumberForAuditAsync(string invoiceNumber);
}