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
        var query = _context.Products
            .AsNoTracking()
            .Where(product => product.Stock > 0)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value))
        {
            var cleanField = field.Trim().ToLowerInvariant();
            var cleanValue = value.Trim().ToUpperInvariant();

            query = cleanField switch
            {
                "id" when int.TryParse(value, out var id) =>
                    query.Where(product => product.Id == id),

                "name" =>
                    query.Where(product => product.Name.Contains(cleanValue)),

                "price" when decimal.TryParse(value, out var price) =>
                    query.Where(product => product.Price == price),

                "stock" when int.TryParse(value, out var stock) =>
                    query.Where(product => product.Stock == stock),

                _ => query
            };
        }

        return await query
            .OrderBy(product => product.Id)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
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