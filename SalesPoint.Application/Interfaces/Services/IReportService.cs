using SalesPoint.Application.DTOs.Reports;

namespace SalesPoint.Application.Interfaces.Services;

public interface IReportService
{
    Task<SalesReportDto> GetAsync(ReportRequest request);
    Task<byte[]> ExportCsvAsync(ReportRequest request);
}
