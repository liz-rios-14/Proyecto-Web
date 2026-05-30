using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Domain.Entities;
using System.Data;

namespace SalesPoint.Infrastructure.Persistence.Repositories;
public sealed class InvoiceRepository : IInvoiceRepository, ISaleRepository
{
    private readonly AppDbContext _context;
    public InvoiceRepository(AppDbContext context) => _context = context;
    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        var lastInvoiceId = await _context.Invoices.OrderByDescending(i => i.Id).Select(i => i.Id).FirstOrDefaultAsync();
        invoice.AssignInvoiceNumber(lastInvoiceId + 1);
        await _context.Invoices.AddAsync(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }
    public async Task UpdateAsync(Invoice invoice) => await _context.SaveChangesAsync();
    public async Task<List<Invoice>> GetAllAsync() => await _context.Invoices.Include(i => i.Details).OrderByDescending(i => i.Date).ToListAsync();
    public async Task<Invoice?> GetByIdAsync(int id) => await _context.Invoices.Include(i => i.Details).FirstOrDefaultAsync(i => i.Id == id);
}
