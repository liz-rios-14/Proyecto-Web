using SalesPoint.Application.DTOs.Auth;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    public AuthService(IUserRepository userRepository) => _userRepository = userRepository;
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        ApplicationValidator.Required(request.UserNameOrEmail, "El usuario o correo");
        ApplicationValidator.Required(request.Password, "La contraseña");
        var user = await _userRepository.GetByUserNameOrEmailAsync(request.UserNameOrEmail) ?? throw new DomainException("Credenciales incorrectas.");
        if (!user.IsActive) throw new DomainException("El usuario está inactivo.");
        if (user.PasswordHash != UserService.Hash(request.Password)) throw new DomainException("Credenciales incorrectas.");
        return new LoginResponseDto { UserId = user.Id, UserName = user.UserName, Email = user.Email, RoleName = user.Role?.Name ?? string.Empty, Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()), Expiration = DateTime.UtcNow.AddHours(8) };
    }
}
