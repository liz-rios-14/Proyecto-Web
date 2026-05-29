using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;
using System.Data;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var lastInvoiceId = await _context.Invoices
                .OrderByDescending(item => item.Id)
                .Select(item => item.Id)
                .FirstOrDefaultAsync();

            invoice.AssignInvoiceNumber(lastInvoiceId + 1);

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return invoice;
        });
    }

    public async Task<List<Invoice>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await _context.Invoices
            .AsNoTracking()
            .AsSplitQuery()
            .Include(invoice => invoice.Details)
            .OrderByDescending(invoice => invoice.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Invoices.CountAsync();
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .AsNoTracking()
            .AsSplitQuery()
            .Include(invoice => invoice.Details)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);
    }
}