using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.Invoices;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR,SELLER")]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 8)
    {
        var result = await _invoiceService.GetAllAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _invoiceService.GetByIdAsync(id);

        if (result is null)
            return NotFound(new { message = "Factura no encontrada." });

        return Ok(result);
    }

    [HttpGet("audit/{invoiceNumber}")]
    public async Task<IActionResult> ReconstructByInvoiceNumber(string invoiceNumber)
    {
        var result = await _invoiceService.ReconstructByInvoiceNumberAsync(invoiceNumber);

        if (result is null)
        {
            return NotFound(new
            {
                message = $"No existe una factura con número {invoiceNumber}."
            });
        }

        return Ok(result);
    }

    [HttpGet("audit-history")]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> GetAuditHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        return Ok(await _invoiceService.GetAuditHistoryAsync(pageNumber, pageSize));
    }

    [HttpPost]
    [Authorize(Roles = "SELLER")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
    {
        var result = await _invoiceService.CreateAsync(request);
        return Ok(result);
    }
}
