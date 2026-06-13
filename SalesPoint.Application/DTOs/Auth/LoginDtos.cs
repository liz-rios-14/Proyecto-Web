namespace SalesPoint.Application.DTOs.Auth;

public sealed class LoginRequestDto
{
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class GoogleLoginRequestDto
{
    public string Credential { get; set; } = string.Empty;
}

public sealed class ExternalAuthenticationStatusDto
{
    public bool GoogleEnabled { get; set; }
    public string GoogleClientId { get; set; } = string.Empty;
}

public sealed class LoginResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserSessionDto User { get; set; } = new();
    public string Role { get; set; } = string.Empty;
}

public sealed class UserSessionDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class LoginResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoginResponseDto? Data { get; set; }

    public static LoginResultDto Success(LoginResponseDto data) =>
        new() { IsSuccess = true, Data = data };

    public static LoginResultDto Failure(string message) =>
        new() { IsSuccess = false, Message = message };
}

public sealed class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public sealed class ForgotPasswordResponseDto
{
    public string? ResetToken { get; set; }
    public string Message { get; set; } =
        "Se generó una solicitud de recuperación.";
}

public sealed class ForgotPasswordResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public ForgotPasswordResponseDto? Data { get; set; }

    public static ForgotPasswordResultDto Success(ForgotPasswordResponseDto data) =>
        new() { IsSuccess = true, Data = data };

    public static ForgotPasswordResultDto Failure(string message) =>
        new() { IsSuccess = false, Message = message };
}

public sealed class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ResetPasswordResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ResetPasswordResultDto Success() => new() { IsSuccess = true };

    public static ResetPasswordResultDto Failure(string message) =>
        new() { IsSuccess = false, Message = message };
}
