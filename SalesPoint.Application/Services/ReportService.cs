using SalesPoint.Application.DTOs.Reports;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Security;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

// Reports / Excel Export
public sealed class ReportService : IReportService
{
    private readonly IReportRepository _repository;
    private readonly ICurrentUserContext _currentUser;

    public ReportService(
        IReportRepository repository,
        ICurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public Task<SalesReportDto> GetAsync(ReportRequest request)
    {
        Validate(request);
        int? sellerId = string.Equals(
            _currentUser.Role,
            "ADMINISTRATOR",
            StringComparison.OrdinalIgnoreCase)
            ? null
            : _currentUser.UserId;

        return _repository.GetAsync(
            request.StartDate.Date,
            request.EndDate.Date.AddDays(1),
            sellerId);
    }

    public async Task<byte[]> ExportExcelAsync(ReportRequest request)
    {
        var report = await GetAsync(request);
        if (report.Totals.InvoiceCount == 0)
            throw new DomainException("No existen datos para exportar.");

        return SalesReportExcelExporter.Create(
            report,
            request.StartDate.Date,
            request.EndDate.Date);
    }

    private static void Validate(ReportRequest request)
    {
        if (request is null || request.StartDate == default || request.EndDate == default)
            throw new DomainException("Seleccione la fecha inicio y la fecha fin.");
        if (request.EndDate.Date < request.StartDate.Date)
            throw new DomainException("La fecha fin no puede ser menor a la fecha inicio.");
        if ((request.EndDate.Date - request.StartDate.Date).TotalDays > 366)
            throw new DomainException("El rango máximo permitido es de 366 días.");
    }
}
