using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;
public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;
    public CustomerRepository(AppDbContext context) => _context = context;
    public async Task<List<Customer>> SearchAsync(string field, string value)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value))
        {
            var cleanField = field.Trim().ToLowerInvariant(); var cleanValue = value.Trim().ToUpperInvariant();
            query = cleanField switch { "id" when int.TryParse(value, out var id) => query.Where(c => c.Id == id), "firstname" => query.Where(c => c.FirstName.Contains(cleanValue)), "lastname" => query.Where(c => c.LastName.Contains(cleanValue)), "phone" => query.Where(c => c.Phone.Contains(value.Trim())), "address" => query.Where(c => c.Address.Contains(cleanValue)), "email" => query.Where(c => c.Email.Contains(value.Trim().ToLowerInvariant())), _ => query };
        }
        return await query.OrderBy(c => c.Id).ToListAsync();
    }
    public async Task<Customer?> GetByIdAsync(int id) => await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        var clean = email.Trim().ToLowerInvariant();
        return await _context.Customers.AnyAsync(c => c.Email == clean && (!excludeId.HasValue || c.Id != excludeId.Value));
    }
    public async Task<Customer> CreateAsync(Customer customer) { await _context.Customers.AddAsync(customer); await _context.SaveChangesAsync(); return customer; }
    public async Task UpdateAsync(Customer customer) => await _context.SaveChangesAsync();
    public async Task DeleteAsync(Customer customer) { _context.Customers.Remove(customer); await _context.SaveChangesAsync(); }
}
