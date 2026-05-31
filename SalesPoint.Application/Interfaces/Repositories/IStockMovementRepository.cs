using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IStockMovementRepository
{
    Task<StockMovement> CreateAsync(StockMovement stockMovement);
    Task<List<StockMovement>> GetByProductIdAsync(int productId);
    Task<List<StockMovement>> GetAllAsync();
}
