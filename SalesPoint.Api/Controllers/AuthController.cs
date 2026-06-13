using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPoint.Application.DTOs.Auth;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _service.LoginAsync(request);
        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new { message = result.Message });
    }

    [AllowAnonymous]
    [HttpGet("external/status")]
    public IActionResult ExternalStatus()
    {
        return Ok(_service.GetExternalAuthenticationStatus());
    }

    [AllowAnonymous]
    [HttpPost("external/google")]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginRequestDto request)
    {
        var result = await _service.LoginWithGoogleAsync(request);
        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new { message = result.Message });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _service.RefreshAsync(request);
        return result.IsSuccess
            ? Ok(result.Data)
            : Unauthorized(new { message = result.Message });
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        await _service.LogoutAsync(request);
        return Ok(new { message = "Sesión cerrada correctamente." });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _service.ForgotPasswordAsync(request);
        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new { message = result.Message });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDto request)
    {
        var result = await _service.ResetPasswordAsync(request);
        return result.IsSuccess
            ? Ok(new { message = "La contraseña fue actualizada correctamente." })
            : BadRequest(new { message = result.Message });
    }
}
