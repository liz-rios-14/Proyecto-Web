using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.Reports;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR,SELLER")]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ReportRequest request) =>
        Ok(await _service.GetAsync(request));

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] ReportRequest request)
    {
        var file = await _service.ExportCsvAsync(request);
        return File(
            file,
            "text/csv; charset=utf-8",
            $"reporte-ventas-{request.StartDate:yyyyMMdd}-{request.EndDate:yyyyMMdd}.csv");
    }
}
