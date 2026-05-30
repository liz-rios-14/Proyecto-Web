using SalesPoint.Application.DTOs.Sales;
namespace SalesPoint.Application.Interfaces.Services;
public interface ISaleService
{
    Task<SaleDto> CreateAsync(SaleCreateDto request);
    Task<SaleDto> ConfirmAsync(int id);
    Task<SaleDto> CancelAsync(int id);
    Task<List<SaleHistoryDto>> GetHistoryAsync();
    Task<SaleDto?> GetInvoiceByIdAsync(int id);
}
