using System.Text;
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

    public async Task<byte[]> ExportCsvAsync(ReportRequest request)
    {
        var report = await GetAsync(request);
        if (report.Totals.InvoiceCount == 0)
            throw new DomainException("No existen datos para exportar.");

        var csv = new StringBuilder();
        csv.AppendLine("REPORTE DE VENTAS");
        csv.AppendLine($"Desde;{request.StartDate:yyyy-MM-dd};Hasta;{request.EndDate:yyyy-MM-dd}");
        csv.AppendLine();
        csv.AppendLine("VENTAS POR FECHA");
        csv.AppendLine("Fecha;Facturas;Subtotal;IVA;Total");
        foreach (var item in report.SalesByDate)
            csv.AppendLine($"{item.Date:yyyy-MM-dd};{item.InvoiceCount};{item.Subtotal:F2};{item.Tax:F2};{item.Total:F2}");

        csv.AppendLine();
        csv.AppendLine("PRODUCTOS MÁS VENDIDOS");
        csv.AppendLine("Producto;Cantidad;Total");
        foreach (var item in report.TopProducts)
            csv.AppendLine($"{Escape(item.ProductName)};{item.Quantity};{item.Total:F2}");

        csv.AppendLine();
        csv.AppendLine("TOTALES");
        csv.AppendLine("Facturas;Subtotal;IVA;Total");
        csv.AppendLine($"{report.Totals.InvoiceCount};{report.Totals.Subtotal:F2};{report.Totals.Tax:F2};{report.Totals.Total:F2}");

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
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

    private static string Escape(string value) =>
        value.Contains(';') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
