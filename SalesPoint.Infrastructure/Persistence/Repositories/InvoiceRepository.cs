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
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var lastInvoiceId = await _context.Invoices
                .OrderByDescending(item => item.Id)
                .Select(item => item.Id)
                .FirstOrDefaultAsync();

            var nextSequence = lastInvoiceId + 1;

            invoice.AssignInvoiceNumber(nextSequence);

            await _context.Invoices.AddAsync(invoice);

            // Bloquea internamente, genera un nuevo número de factura y guarda factura + stock actualizado.
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

    public async Task<List<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(invoice => invoice.Details)
            .OrderByDescending(invoice => invoice.Date)
            .ToListAsync();
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(invoice => invoice.Details)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);
    }
}