using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.Products;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR,SELLER")]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string field = "",
        [FromQuery] string value = "",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool onlyAvailable = false)
    {
        onlyAvailable = onlyAvailable || User.IsInRole("SELLER");
        var result = await _service.SearchAsync(field, value, pageNumber, pageSize, onlyAvailable);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Producto no encontrado." });

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _service.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        await _service.UpdateAsync(id, request);
        return Ok(new { message = "Producto actualizado correctamente." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await _service.DeleteAsync(id));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Deactivate(int id)
    {
        return Ok(await _service.DeactivateAsync(id));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Activate(int id)
    {
        return Ok(await _service.ActivateAsync(id));
    }
}
