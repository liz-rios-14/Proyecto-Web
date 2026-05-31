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

    public async Task<List<Product>> SearchAsync(string field, string value)
    {
        return await SearchAsync(field, value, 1, 8);
    }

    public async Task<List<Product>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize)
    {
        var query = BuildSearchQuery(field, value);

        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        
        return await query
            .OrderBy(product => product.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string field, string value)
    {
        var query = BuildSearchQuery(field, value);
        return await query.CountAsync();
    }

    private IQueryable<Product> BuildSearchQuery(string field, string value)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(product => product.Stock > 0)
            .AsQueryable();

        if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value))
            return query;

        var cleanField = field.Trim().ToLowerInvariant();
        var cleanValue = value.Trim();

        if (cleanField == "id" && int.TryParse(cleanValue, out var id))
            return query.Where(product => product.Id == id);

        if (cleanField == "name")
            return query.Where(product => product.Name.Contains(cleanValue.ToUpper()));

        if (cleanField == "price" && decimal.TryParse(cleanValue, out var price))
            return query.Where(product => product.Price == price);

        if (cleanField == "stock" && int.TryParse(cleanValue, out var stock))
            return query.Where(product => product.Stock == stock);

        return query;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}