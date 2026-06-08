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

public sealed class LoginResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoginResponseDto? Data { get; set; }

    public static LoginResultDto Success(LoginResponseDto data)
    {
        return new LoginResultDto
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static LoginResultDto Failure(string message)
    {
        return new LoginResultDto
        {
            IsSuccess = false,
            Message = message
        };
    }
}

public sealed class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public sealed class ForgotPasswordResponseDto
{
    public string ResetToken { get; set; } = string.Empty;
}

public sealed class ForgotPasswordResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public ForgotPasswordResponseDto? Data { get; set; }

    public static ForgotPasswordResultDto Success(ForgotPasswordResponseDto data)
    {
        return new ForgotPasswordResultDto
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static ForgotPasswordResultDto Failure(string message)
    {
        return new ForgotPasswordResultDto
        {
            IsSuccess = false,
            Message = message
        };
    }
}

public sealed class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class ResetPasswordResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ResetPasswordResultDto Success()
    {
        return new ResetPasswordResultDto { IsSuccess = true };
    }

    public static ResetPasswordResultDto Failure(string message)
    {
        return new ResetPasswordResultDto
        {
            IsSuccess = false,
            Message = message
        };
    }
}
