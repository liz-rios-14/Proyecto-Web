using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;
using System.Data;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository, ISaleRepository
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

            foreach (var detail in invoice.Details)
            {
                var product = await _context.Products
                    .FirstAsync(item => item.Id == detail.ProductId);

                await _context.StockMovements.AddAsync(new StockMovement(
                    detail.ProductId,
                    "SALE_CONFIRMED",
                    detail.Quantity * -1,
                    product.Stock,
                    $"Venta confirmada {invoice.InvoiceNumber}",
                    invoiceId: invoice.Id));
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return invoice;
        });
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .AsNoTracking()
            .AsSplitQuery()
            .Include(invoice => invoice.Details)
            .OrderByDescending(invoice => invoice.Date)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetAllAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

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