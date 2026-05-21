using SalesPoint.Application.DTOs.Invoices;

namespace SalesPoint.Application.Interfaces.Services;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request);
    Task<List<InvoiceHistoryDto>> GetAllAsync();
    Task<InvoiceDto?> GetByIdAsync(int id);
}