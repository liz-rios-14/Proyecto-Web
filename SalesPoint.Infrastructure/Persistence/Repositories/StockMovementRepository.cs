using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class StockMovementRepository : IStockMovementRepository
{
    private readonly AppDbContext _context;

    public StockMovementRepository(AppDbContext context) => _context = context;

    public async Task<StockMovement> CreateAsync(StockMovement stockMovement)
    {
        await _context.StockMovements.AddAsync(stockMovement);
        await _context.SaveChangesAsync();
        return stockMovement;
    }

    public async Task<List<StockMovement>> GetByProductIdAsync(int productId) =>
        await _context.StockMovements.AsNoTracking()
            .Where(movement => movement.ProductId == productId)
            .OrderByDescending(movement => movement.CreatedAt)
            .ToListAsync();

    public async Task<List<StockMovement>> GetAllAsync() =>
        await _context.StockMovements.AsNoTracking()
            .OrderByDescending(movement => movement.CreatedAt)
            .ToListAsync();
}
