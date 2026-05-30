using SalesPoint.Application.DTOs.StockMovements;
namespace SalesPoint.Application.Interfaces.Services;
public interface IStockMovementService
{
    Task<List<StockMovementDto>> GetAllAsync();
    Task<List<StockMovementDto>> GetByProductIdAsync(int productId);
    Task<StockMovementDto> CreateAsync(CreateStockMovementRequest request);
}
