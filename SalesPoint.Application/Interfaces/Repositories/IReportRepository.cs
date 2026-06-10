using SalesPoint.Application.DTOs.Reports;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<SalesReportDto> GetAsync(
        DateTime startDate,
        DateTime exclusiveEndDate,
        int? sellerId);
}
