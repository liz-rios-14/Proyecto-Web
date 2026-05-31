using SalesPoint.Domain.Entities;
namespace SalesPoint.Application.Interfaces.Repositories;
public interface IStockMovementRepository
{
    Task<List<StockMovement>> GetAllAsync();
    Task<List<StockMovement>> GetByProductIdAsync(int productId);
    Task<StockMovement> CreateAsync(StockMovement movement);
}
