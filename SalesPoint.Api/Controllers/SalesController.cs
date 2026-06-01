using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.Sales;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR,SELLER")]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _service;

    public SalesController(ISaleService service)
    {
        _service = service;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        return Ok(await _service.GetHistoryAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return (await _service.GetInvoiceByIdAsync(id)) is { } result
            ? Ok(result)
            : NotFound("Factura no encontrada.");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleCreateDto request)
    {
        return Ok(await _service.CreateAsync(request));
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        return Ok(await _service.ConfirmAsync(id));
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        return Ok(await _service.CancelAsync(id));
    }
}
