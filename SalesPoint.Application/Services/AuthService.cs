using SalesPoint.Application.DTOs.Auth;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Security;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        ApplicationValidator.Required(request.UserNameOrEmail, "El usuario o correo");
        ApplicationValidator.Required(request.Password, "La contrasena");

        var user = await _userRepository.GetByUserNameOrEmailAsync(request.UserNameOrEmail)
            ?? throw new DomainException("Credenciales incorrectas.");

        if (!user.IsActive)
            throw new DomainException("El usuario esta inactivo.");

        if (user.PasswordHash != UserService.Hash(request.Password))
            throw new DomainException("Credenciales incorrectas.");

        var roleName = user.Role?.Name ?? string.Empty;
        var token = _jwtTokenGenerator.Generate(user, roleName, out var expiration);

        return new LoginResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            RoleName = roleName,
            Token = token,
            Expiration = expiration
        };
    }
}
