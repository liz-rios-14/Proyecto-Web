using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice> CreateAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
    Task<List<Invoice>> GetAllAsync();
    Task<List<Invoice>> GetAllAsync(
        int pageNumber,
        int pageSize,
        int? sellerId = null);
    Task<int> CountAsync(int? sellerId = null);
    Task<Invoice?> GetByIdAsync(int id, int? sellerId = null);
    Task<Invoice?> GetByInvoiceNumberForAuditAsync(
        string invoiceNumber,
        int? sellerId = null);
    Task<AuditInvoiceHistory> CreateAuditHistoryAsync(AuditInvoiceHistory history);
    Task<List<AuditInvoiceHistory>> GetAuditHistoryAsync(int pageNumber, int pageSize);
    Task<int> CountAuditHistoryAsync();
}
