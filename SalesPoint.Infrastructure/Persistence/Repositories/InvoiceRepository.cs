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

    public async Task<List<Invoice>> GetAllAsync(
        int pageNumber,
        int pageSize,
        int? sellerId = null)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        var query = _context.Invoices
            .AsNoTracking()
            .AsSplitQuery()
            .Include(invoice => invoice.Details)
            .AsQueryable();

        if (sellerId.HasValue)
            query = query.Where(invoice => invoice.SellerId == sellerId.Value);

        return await query
            .OrderByDescending(invoice => invoice.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(int? sellerId = null)
    {
        return sellerId.HasValue
            ? await _context.Invoices.CountAsync(
                invoice => invoice.SellerId == sellerId.Value)
            : await _context.Invoices.CountAsync();
    }

    public async Task<Invoice?> GetByIdAsync(int id, int? sellerId = null)
    {
        return await _context.Invoices
            .AsNoTracking()
            .AsSplitQuery()
            .Include(invoice => invoice.Details)
            .FirstOrDefaultAsync(invoice =>
                invoice.Id == id &&
                (!sellerId.HasValue || invoice.SellerId == sellerId.Value));
    }

    async Task<Invoice?> ISaleRepository.GetByIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<Invoice?> GetByInvoiceNumberForAuditAsync(
        string invoiceNumber,
        int? sellerId = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return null;

        var cleanInvoiceNumber = invoiceNumber.Trim().ToUpperInvariant();

        return await _context.Invoices
            .AsNoTracking()
            .AsSplitQuery()
            .Include(invoice => invoice.Details)
            .FirstOrDefaultAsync(invoice =>
                invoice.InvoiceNumber.ToUpper() == cleanInvoiceNumber &&
                (!sellerId.HasValue || invoice.SellerId == sellerId.Value));
    }
}
