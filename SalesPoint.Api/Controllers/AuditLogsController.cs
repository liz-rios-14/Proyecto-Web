using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.AuditLogs;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR")]
[Route("api/audit-logs")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] AuditLogSearchRequest request) =>
        Ok(await _service.SearchAsync(request));
}
