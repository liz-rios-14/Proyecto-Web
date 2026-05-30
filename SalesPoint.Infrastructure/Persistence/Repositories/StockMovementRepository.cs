using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;
namespace SalesPoint.Infrastructure.Persistence.Repositories;
public sealed class StockMovementRepository : IStockMovementRepository
{
    private readonly AppDbContext _context;
    public StockMovementRepository(AppDbContext context) => _context = context;
    public async Task<List<StockMovement>> GetAllAsync() => await _context.StockMovements.Include(s => s.Product).AsNoTracking().OrderByDescending(s => s.CreatedAt).ToListAsync();
    public async Task<List<StockMovement>> GetByProductIdAsync(int productId) => await _context.StockMovements.Include(s => s.Product).AsNoTracking().Where(s => s.ProductId == productId).OrderByDescending(s => s.CreatedAt).ToListAsync();
    public async Task<StockMovement> CreateAsync(StockMovement movement) { await _context.StockMovements.AddAsync(movement); await _context.SaveChangesAsync(); return movement; }
}
