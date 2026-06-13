using System.IO.Compression;
using SalesPoint.Application.DTOs.Reports;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Security;
using SalesPoint.Application.Services;
using Xunit;

namespace SalesPoint.Tests;

public sealed class ReportServiceTests
{
    [Fact]
    public async Task ExportExcel_CreatesWorkbookWithReportSheets()
    {
        var repository = new ReportRepositoryStub(CreateReport());
        var service = new ReportService(
            repository,
            new CurrentUserStub(1, "admin", "ADMINISTRATOR"));

        var file = await service.ExportExcelAsync(new ReportRequest
        {
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 12)
        });

        using var stream = new MemoryStream(file);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var workbook = archive.GetEntry("xl/workbook.xml");
        var sheets = archive.Entries.Count(entry =>
            entry.FullName.StartsWith(
                "xl/worksheets/sheet",
                StringComparison.Ordinal));

        Assert.NotNull(workbook);
        Assert.True(workbook!.Length > 0);
        Assert.Equal(6, sheets);
    }

    [Fact]
    public async Task GetAsync_FiltersReportsForSeller()
    {
        var repository = new ReportRepositoryStub(CreateReport());
        var service = new ReportService(
            repository,
            new CurrentUserStub(27, "seller", "SELLER"));

        await service.GetAsync(new ReportRequest
        {
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 12)
        });

        Assert.Equal(27, repository.LastSellerId);
    }

    private static SalesReportDto CreateReport() =>
        new()
        {
            Totals = new ReportTotalsDto
            {
                InvoiceCount = 1,
                Subtotal = 10m,
                Tax = 1.20m,
                Total = 11.20m
            },
            SalesByDate =
            [
                new SalesByDateDto
                {
                    Date = new DateTime(2026, 6, 12),
                    InvoiceCount = 1,
                    Subtotal = 10m,
                    Tax = 1.20m,
                    Total = 11.20m
                }
            ]
        };

    private sealed class ReportRepositoryStub : IReportRepository
    {
        private readonly SalesReportDto _report;

        public ReportRepositoryStub(SalesReportDto report)
        {
            _report = report;
        }

        public int? LastSellerId { get; private set; }

        public Task<SalesReportDto> GetAsync(
            DateTime startDate,
            DateTime exclusiveEndDate,
            int? sellerId)
        {
            LastSellerId = sellerId;
            return Task.FromResult(_report);
        }
    }

    private sealed class CurrentUserStub : ICurrentUserContext
    {
        public CurrentUserStub(int userId, string userName, string role)
        {
            UserId = userId;
            UserName = userName;
            Role = role;
        }

        public int UserId { get; }
        public string UserName { get; }
        public string Role { get; }
    }
}
