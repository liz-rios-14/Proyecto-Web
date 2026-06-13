using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMINISTRATOR,SELLER")]
[Route("api/system-status")]
public sealed class SystemStatusController : ControllerBase
{
    private readonly ISystemStatusService _service;
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public SystemStatusController(
        ISystemStatusService service,
        IAuthService authService,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _service = service;
        _authService = authService;
        _environment = environment;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var status = await _service.GetAsync();
        var externalAuthentication =
            _authService.GetExternalAuthenticationStatus();

        return Ok(new
        {
            frontendConnected = true,
            apiConnected = true,
            status.DatabaseConnected,
            googleConfigured = externalAuthentication.GoogleEnabled,
            environment = _environment.EnvironmentName,
            deploymentMode =
                _configuration["Deployment:Mode"] ?? "Ejecución local",
            runningInContainer =
                string.Equals(
                    Environment.GetEnvironmentVariable(
                        "DOTNET_RUNNING_IN_CONTAINER"),
                    "true",
                    StringComparison.OrdinalIgnoreCase),
            status.CheckedAt
        });
    }
}
