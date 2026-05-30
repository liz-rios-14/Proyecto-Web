using SalesPoint.Application.DTOs.Auth;
namespace SalesPoint.Application.Interfaces.Services;
public interface IAuthService { Task<LoginResponseDto> LoginAsync(LoginRequestDto request); }
