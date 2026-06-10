using Microsoft.EntityFrameworkCore;
using SalesPoint.Application.DTOs.Reports;
using SalesPoint.Application.Interfaces.Repositories;

namespace SalesPoint.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReportDto> GetAsync(
        DateTime startDate,
        DateTime exclusiveEndDate,
        int? sellerId)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(item => item.Details)
            .Where(item => item.Date >= startDate && item.Date < exclusiveEndDate);

        if (sellerId.HasValue)
            query = query.Where(item => item.SellerId == sellerId.Value);

        var invoices = await query.ToListAsync();
        var lowStock = await _context.Products
            .AsNoTracking()
            .Where(item => item.IsActive && !item.IsDeleted && item.Stock <= 10)
            .OrderBy(item => item.Stock)
            .Select(item => new LowStockProductDto
            {
                ProductId = item.Id,
                ProductName = item.Name,
                Stock = item.Stock
            })
            .ToListAsync();

        return new SalesReportDto
        {
            SalesByDate = invoices
                .GroupBy(item => item.Date.Date)
                .OrderBy(item => item.Key)
                .Select(group => new SalesByDateDto
                {
                    Date = group.Key,
                    InvoiceCount = group.Count(),
                    Subtotal = group.Sum(item => item.Subtotal),
                    Tax = group.Sum(item => item.Tax),
                    Total = group.Sum(item => item.Total)
                }).ToList(),
            SalesBySeller = invoices
                .GroupBy(item => new { item.SellerId, item.SellerFullNameSnapshot })
                .OrderByDescending(group => group.Sum(item => item.Total))
                .Select(group => new SalesBySellerDto
                {
                    SellerId = group.Key.SellerId,
                    SellerName = group.Key.SellerFullNameSnapshot,
                    InvoiceCount = group.Count(),
                    Total = group.Sum(item => item.Total)
                }).ToList(),
            TopProducts = invoices
                .SelectMany(item => item.Details)
                .GroupBy(item => new { item.ProductId, item.ProductName })
                .OrderByDescending(group => group.Sum(item => item.Quantity))
                .Take(20)
                .Select(group => new TopProductDto
                {
                    ProductId = group.Key.ProductId,
                    ProductName = group.Key.ProductName,
                    Quantity = group.Sum(item => item.Quantity),
                    Total = group.Sum(item => item.Subtotal)
                }).ToList(),
            LowStockProducts = lowStock,
            TopCustomers = invoices
                .GroupBy(item => new { item.CustomerId, item.CustomerNameSnapshot })
                .OrderByDescending(group => group.Sum(item => item.Total))
                .Take(20)
                .Select(group => new TopCustomerDto
                {
                    CustomerId = group.Key.CustomerId,
                    CustomerName = group.Key.CustomerNameSnapshot,
                    InvoiceCount = group.Count(),
                    Total = group.Sum(item => item.Total)
                }).ToList(),
            Totals = new ReportTotalsDto
            {
                InvoiceCount = invoices.Count,
                Subtotal = invoices.Sum(item => item.Subtotal),
                Tax = invoices.Sum(item => item.Tax),
                Total = invoices.Sum(item => item.Total)
            }
        };
    }
}
