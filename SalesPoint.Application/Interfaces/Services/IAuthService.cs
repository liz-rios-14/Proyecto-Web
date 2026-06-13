using SalesPoint.Application.DTOs.Auth;

namespace SalesPoint.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);
    ExternalAuthenticationStatusDto GetExternalAuthenticationStatus();
    Task<LoginResultDto> LoginWithGoogleAsync(GoogleLoginRequestDto request);
    Task<LoginResultDto> RefreshAsync(RefreshTokenRequestDto request);
    Task LogoutAsync(LogoutRequestDto request);
    Task<ForgotPasswordResultDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ResetPasswordResultDto> ResetPasswordAsync(ResetPasswordRequestDto request);
}
