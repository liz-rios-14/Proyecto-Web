using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.Customers;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR,SELLER")]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string field = "",
        [FromQuery] string value = "",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool onlyActive = false)
    {
        onlyActive = onlyActive || User.IsInRole("SELLER");
        var result = await _service.SearchAsync(field, value, pageNumber, pageSize, onlyActive);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result is null)
            return NotFound(new { message = "Cliente no encontrado." });

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRATOR,SELLER")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var result = await _service.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest request)
    {
        await _service.UpdateAsync(id, request);
        return Ok(new { message = "Cliente actualizado correctamente." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await _service.DeleteAsync(id));
    }
}
