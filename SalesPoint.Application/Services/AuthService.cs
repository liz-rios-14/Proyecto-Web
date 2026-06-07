using System.Security.Cryptography;
using System.Text.RegularExpressions;
using SalesPoint.Application.DTOs.Auth;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Security;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
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

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        if (request is null)
            return LoginResultDto.Failure(
                "Los datos de inicio de sesión son obligatorios.");

        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail))
            return LoginResultDto.Failure("Ingrese su usuario o correo.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return LoginResultDto.Failure("Ingrese su contraseña.");

        var user = await _userRepository
            .GetByUserNameOrEmailAsync(request.UserNameOrEmail);

        if (user is null)
            return LoginResultDto.Failure("Credenciales incorrectas.");

        if (!user.IsActive || user.IsDeleted)
        {
            return LoginResultDto.Failure(
                "El usuario está inactivo o eliminado.");
        }

        if (user.IsLocked)
        {
            return LoginResultDto.Failure(
                "El usuario está bloqueado. Solicite al administrador que lo desbloquee.");
        }

        if (user.PasswordHash != UserService.Hash(request.Password))
        {
            user.RegisterFailedLoginAttempt();
            await _userRepository.UpdateAsync(user);

            if (user.IsLocked)
            {
                return LoginResultDto.Failure(
                    "Usuario bloqueado por 3 intentos fallidos. Solicite el desbloqueo al administrador.");
            }

            var remainingAttempts = 3 - user.FailedLoginAttempts;
            return LoginResultDto.Failure(
                $"Credenciales incorrectas. Intentos restantes: {remainingAttempts}.");
        }

        if (user.FailedLoginAttempts > 0)
        {
            user.ResetFailedLoginAttempts();
            await _userRepository.UpdateAsync(user);
        }

        var roleName = user.Role?.Name ?? string.Empty;
        var token = _jwtTokenGenerator.Generate(user, roleName, out var expiration);

        return LoginResultDto.Success(
            new LoginResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RoleName = roleName,
                Token = token,
                Expiration = expiration
            });
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        ApplicationValidator.Required(request.Email, "El correo");

        var user = await _userRepository.GetByEmailAsync(request.Email)
            ?? throw new DomainException("No existe un usuario con ese correo.");

        if (!user.IsActive)
            throw new DomainException("El usuario esta inactivo.");

        var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var resetTokenHash = UserService.Hash(resetToken);

        user.SetPasswordResetToken(resetTokenHash, DateTime.UtcNow.AddMinutes(15));

        await _userRepository.UpdateAsync(user);

        return new ForgotPasswordResponseDto
        {
            ResetToken = resetToken
        };
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        ApplicationValidator.Required(request.Email, "El correo");
        ApplicationValidator.Required(request.ResetToken, "El token");
        ApplicationValidator.Required(request.NewPassword, "La nueva contrasena");

        ValidatePasswordPolicy(request.NewPassword);

        var user = await _userRepository.GetByEmailAsync(request.Email)
            ?? throw new DomainException("Usuario no encontrado.");

        if (!user.IsActive)
            throw new DomainException("El usuario esta inactivo.");

        if (user.PasswordResetTokenHash is null || user.PasswordResetTokenExpiresAt is null)
            throw new DomainException("No existe una solicitud de recuperacion activa.");

        if (user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            throw new DomainException("El token de recuperacion expiro.");

        var tokenHash = UserService.Hash(request.ResetToken);

        if (tokenHash != user.PasswordResetTokenHash)
            throw new DomainException("Token de recuperacion invalido.");

        var newPasswordHash = UserService.Hash(request.NewPassword);

        if (newPasswordHash == user.PasswordHash)
            throw new DomainException("No puede usar la contrasena actual.");

        var previousHashes = await _userRepository.GetPasswordHistoryHashesAsync(user.Id);

        if (previousHashes.Contains(newPasswordHash))
            throw new DomainException("No puede usar una contrasena anterior.");

        await _userRepository.AddPasswordHistoryAsync(
            new PasswordHistory(user.Id, user.PasswordHash)
        );

        user.ChangePassword(newPasswordHash);
        user.ClearPasswordResetToken();

        await _userRepository.UpdateAsync(user);
    }

    private static void ValidatePasswordPolicy(string password)
    {
        if (password.Length < 8 || password.Length > 10)
            throw new DomainException("La contrasena debe tener minimo 8 y maximo 10 caracteres.");

        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new DomainException("La contrasena debe tener al menos una mayuscula.");

        if (!Regex.IsMatch(password, "[a-z]"))
            throw new DomainException("La contrasena debe tener al menos una minuscula.");

        if (!Regex.IsMatch(password, "[0-9]"))
            throw new DomainException("La contrasena debe tener al menos un numero.");

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            throw new DomainException("La contrasena debe tener al menos un caracter especial.");
    }
}
