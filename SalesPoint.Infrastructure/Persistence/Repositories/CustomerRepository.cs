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
        return await SearchAsync(field, value, 1, 8);
    }

    public async Task<List<Customer>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize)
    {
        var query = BuildSearchQuery(field, value);

        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        return await query
            .OrderBy(customer => customer.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string field, string value)
    {
        var query = BuildSearchQuery(field, value);
        return await query.CountAsync();
    }

    private IQueryable<Customer> BuildSearchQuery(string field, string value)
    {
        var query = _context.Customers
            .AsNoTracking()
            .AsQueryable();

        if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value))
            return query;

        var cleanField = field.Trim().ToLowerInvariant();
        var cleanValue = value.Trim();

        if (cleanField == "id" && int.TryParse(cleanValue, out var id))
            return query.Where(customer => customer.Id == id);

        if (cleanField == "firstname")
            return query.Where(customer => customer.FirstName.Contains(cleanValue.ToUpper()));

        if (cleanField == "lastname")
            return query.Where(customer => customer.LastName.Contains(cleanValue.ToUpper()));

        if (cleanField == "phone")
            return query.Where(customer => customer.Phone.Contains(cleanValue));

        if (cleanField == "address")
            return query.Where(customer => customer.Address.Contains(cleanValue.ToUpper()));

        if (cleanField == "email")
            return query.Where(customer => customer.Email.Contains(cleanValue.ToLower()));

        return query;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .AsNoTracking()
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