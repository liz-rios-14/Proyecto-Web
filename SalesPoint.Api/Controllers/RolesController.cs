using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.Roles;
using SalesPoint.Application.Interfaces.Services;
namespace SalesPoint.Api.Controllers;
[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;
    public RolesController(IRoleService service) => _service = service;
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateRoleRequest request) => Ok(await _service.CreateAsync(request));
}
