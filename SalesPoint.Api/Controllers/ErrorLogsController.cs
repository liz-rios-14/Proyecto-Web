using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.ErrorLogs;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR")]
[Route("api/error-logs")]
public sealed class ErrorLogsController : ControllerBase
{
    private readonly IErrorLogService _service;

    public ErrorLogsController(IErrorLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ErrorLogQuery query)
    {
        return Ok(await _service.SearchAsync(query));
    }
}
