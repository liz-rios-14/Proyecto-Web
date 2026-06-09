using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> SearchAsync(string field, string value, bool onlyAvailable)
    {
        return await SearchAsync(field, value, 1, 8, onlyAvailable);
    }

    public async Task<List<Product>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize,
        bool onlyAvailable)
    {
        var query = BuildSearchQuery(field, value, onlyAvailable);

        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        return await query
            .OrderBy(product => product.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string field, string value, bool onlyAvailable)
    {
        var query = BuildSearchQuery(field, value, onlyAvailable);
        return await query.CountAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        var cleanName = name.Trim().ToUpperInvariant();

        return await _context.Products
            .FirstOrDefaultAsync(product => product.Name == cleanName);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var cleanName = name.Trim().ToUpperInvariant();

        return await _context.Products
            .AsNoTracking()
            .AnyAsync(product =>
                product.Name == cleanName &&
                (!excludeId.HasValue || product.Id != excludeId.Value));
    }

    public async Task<bool> HasHistoryAsync(int id)
    {
        return await _context.Set<InvoiceDetail>().AnyAsync(detail => detail.ProductId == id) ||
               await _context.SaleDetails.AnyAsync(detail => detail.ProductId == id) ||
               await _context.StockMovements.AnyAsync(movement => movement.ProductId == id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Product> BuildSearchQuery(string field, string value, bool onlyAvailable)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(product => !product.IsDeleted)
            .AsQueryable();

        if (onlyAvailable)
            query = query.Where(product => product.IsActive && product.Stock > 0);

        if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value))
            return query;

        var cleanField = field.Trim().ToLowerInvariant();
        var cleanValue = value.Trim();

        if (cleanField == "id" && int.TryParse(cleanValue, out var id))
            return query.Where(product => product.Id == id);

        if (cleanField == "name")
            return query.Where(product => product.Name.StartsWith(cleanValue.ToUpperInvariant()));

        if (cleanField == "price")
        {
            var normalizedPrice = cleanValue.Replace(",", ".");
            return query.Where(product => product.Price.ToString().StartsWith(normalizedPrice));
        }

        if (cleanField == "stock")
            return query.Where(product => product.Stock.ToString().StartsWith(cleanValue));

        return query;
    }
}
