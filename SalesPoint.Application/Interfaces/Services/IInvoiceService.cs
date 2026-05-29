using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.DTOs.Invoices;

namespace SalesPoint.Application.Interfaces.Services;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request);
    Task<PagedResponse<InvoiceHistoryDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<InvoiceDto?> GetByIdAsync(int id);
}