using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.Users;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR")]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return (await _service.GetByIdAsync(id)) is { } result
            ? Ok(result)
            : NotFound(new { message = "Usuario no encontrado." });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        return Ok(await _service.CreateAsync(request));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        await _service.UpdateAsync(id, request);
        return Ok(new { message = "Usuario actualizado correctamente." });
    }

    [HttpPost("{id}/disable")]
    public async Task<IActionResult> Disable(int id)
    {
        await _service.DisableAsync(id);
        return Ok(new { message = "Usuario desactivado correctamente." });
    }

    [HttpPost("{id}/unlock")]
    public async Task<IActionResult> Unlock(int id)
    {
        await _service.UnlockAsync(id);
        return Ok(new { message = "Usuario desbloqueado correctamente." });
    }
}
