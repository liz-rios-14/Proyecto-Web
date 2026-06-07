using SalesPoint.Application.DTOs.Auth;

namespace SalesPoint.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);
    Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
}
