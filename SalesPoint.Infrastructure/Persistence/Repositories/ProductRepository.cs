using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;
public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context) => _context = context;
    public async Task<List<Product>> SearchAsync(string field, string value)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value))
        {
            var cleanField = field.Trim().ToLowerInvariant(); var cleanValue = value.Trim().ToUpperInvariant();
            query = cleanField switch { "id" when int.TryParse(value, out var id) => query.Where(p => p.Id == id), "name" => query.Where(p => p.Name.Contains(cleanValue)), "price" when decimal.TryParse(value, out var price) => query.Where(p => p.Price == price), "stock" when int.TryParse(value, out var stock) => query.Where(p => p.Stock == stock), _ => query };
        }
        return await query.OrderBy(p => p.Id).ToListAsync();
    }
    public async Task<Product?> GetByIdAsync(int id) => await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    public async Task<Product?> GetByNameAsync(string name) => await _context.Products.FirstOrDefaultAsync(p => p.Name == name.Trim().ToUpperInvariant());
    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var clean = name.Trim().ToUpperInvariant();
        return await _context.Products.AnyAsync(p => p.Name == clean && (!excludeId.HasValue || p.Id != excludeId.Value));
    }
    public async Task<Product> CreateAsync(Product product) { await _context.Products.AddAsync(product); await _context.SaveChangesAsync(); return product; }
    public async Task UpdateAsync(Product product) => await _context.SaveChangesAsync();
    public async Task DeleteAsync(Product product) { _context.Products.Remove(product); await _context.SaveChangesAsync(); }
}
