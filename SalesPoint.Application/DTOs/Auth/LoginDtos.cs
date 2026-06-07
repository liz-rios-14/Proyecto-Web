namespace SalesPoint.Application.DTOs.Auth;

public sealed class LoginRequestDto
{
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}

public sealed class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public sealed class ForgotPasswordResponseDto
{
    public string ResetToken { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}