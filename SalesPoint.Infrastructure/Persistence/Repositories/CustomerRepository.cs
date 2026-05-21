using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> SearchAsync(string field, string value)
    {
        var query = _context.Customers
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value))
        {
            var cleanField = field.Trim().ToLowerInvariant();
            var cleanValue = value.Trim().ToUpperInvariant();

            query = cleanField switch
            {
                "id" when int.TryParse(value, out var id) =>
                    query.Where(customer => customer.Id == id),

                "firstname" =>
                    query.Where(customer => customer.FirstName.Contains(cleanValue)),

                "lastname" =>
                    query.Where(customer => customer.LastName.Contains(cleanValue)),

                "phone" =>
                    query.Where(customer => customer.Phone.Contains(value.Trim())),

                "address" =>
                    query.Where(customer => customer.Address.Contains(cleanValue)),

                "email" =>
                    query.Where(customer => customer.Email.Contains(value.Trim().ToLowerInvariant())),

                _ => query
            };
        }

        return await query
            .OrderBy(customer => customer.Id)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(customer => customer.Id == id);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }
}