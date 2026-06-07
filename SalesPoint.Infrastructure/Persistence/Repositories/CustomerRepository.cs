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

    public async Task<List<Customer>> SearchAsync(string field, string value, bool onlyActive)
    {
        return await SearchAsync(field, value, 1, 8, onlyActive);
    }

    public async Task<List<Customer>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize,
        bool onlyActive)
    {
        var query = BuildSearchQuery(field, value, onlyActive);

        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        return await query
            .OrderBy(customer => customer.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string field, string value, bool onlyActive)
    {
        var query = BuildSearchQuery(field, value, onlyActive);
        return await query.CountAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(customer => customer.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        var cleanEmail = email.Trim().ToLowerInvariant();

        return await _context.Customers
            .AsNoTracking()
            .AnyAsync(customer =>
                customer.Email == cleanEmail &&
                (!excludeId.HasValue || customer.Id != excludeId.Value));
    }

    public async Task<bool> ExistsByCedulaAsync(string cedula, int? excludeId = null)
    {
        var cleanCedula = cedula.Trim();

        return await _context.Customers
            .AsNoTracking()
            .AnyAsync(customer =>
                customer.Cedula == cleanCedula &&
                (!excludeId.HasValue || customer.Id != excludeId.Value));
    }

    public async Task<bool> HasHistoryAsync(int id)
    {
        return await _context.Invoices.AnyAsync(invoice => invoice.CustomerId == id) ||
               await _context.Sales.AnyAsync(sale => sale.CustomerId == id);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Customer> BuildSearchQuery(string field, string value, bool onlyActive)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Where(customer => !customer.IsDeleted)
            .AsQueryable();

        if (onlyActive)
            query = query.Where(customer => customer.IsActive);

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

        if (cleanField == "cedula")
            return query.Where(customer =>
                customer.Cedula != null &&
                customer.Cedula.Contains(cleanValue));

        if (cleanField == "phone")
            return query.Where(customer => customer.Phone.Contains(cleanValue));

        if (cleanField == "address")
            return query.Where(customer => customer.Address.Contains(cleanValue.ToUpper()));

        if (cleanField == "email")
            return query.Where(customer => customer.Email.Contains(cleanValue.ToLowerInvariant()));

        return query;
    }
}
