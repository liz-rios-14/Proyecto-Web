using System.Security.Cryptography;
using System.Text.RegularExpressions;
using SalesPoint.Application.DTOs.Auth;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Security;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Domain.Entities;

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
            return LoginResultDto.Failure("Correo o contraseña incorrectos.");

        if (!user.IsActive || user.IsDeleted)
        {
            return LoginResultDto.Failure(
                "El usuario se encuentra inactivo.");
        }

        if (user.IsLocked)
        {
            return LoginResultDto.Failure(
                "El usuario se encuentra bloqueado. Contacte al administrador.");
        }

        if (user.PasswordHash != UserService.Hash(request.Password))
        {
            user.RegisterFailedLoginAttempt();
            await _userRepository.UpdateAsync(user);

            if (user.IsLocked)
            {
                return LoginResultDto.Failure(
                    "El usuario se encuentra bloqueado. Contacte al administrador.");
            }

            var remainingAttempts = 3 - user.FailedLoginAttempts;
            return LoginResultDto.Failure(
                $"Correo o contraseña incorrectos. Intentos restantes: {remainingAttempts}.");
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

    public async Task<ForgotPasswordResultDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
            return ForgotPasswordResultDto.Failure("Ingrese su correo.");

        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
            return ForgotPasswordResultDto.Failure("No existe un usuario con ese correo.");

        if (!user.IsActive || user.IsDeleted)
            return ForgotPasswordResultDto.Failure("El usuario se encuentra inactivo.");

        var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var resetTokenHash = UserService.Hash(resetToken);

        user.SetPasswordResetToken(resetTokenHash, DateTime.UtcNow.AddMinutes(15));

        await _userRepository.UpdateAsync(user);

        return ForgotPasswordResultDto.Success(
            new ForgotPasswordResponseDto
            {
                ResetToken = resetToken
            });
    }

    public async Task<ResetPasswordResultDto> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        if (request is null)
            return ResetPasswordResultDto.Failure(
                "Los datos para cambiar la contraseña son obligatorios.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return ResetPasswordResultDto.Failure("Ingrese su correo.");

        if (string.IsNullOrWhiteSpace(request.ResetToken))
            return ResetPasswordResultDto.Failure(
                "Ingrese el token de recuperación.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return ResetPasswordResultDto.Failure("Ingrese la nueva contraseña.");

        var passwordValidationMessage = GetPasswordValidationMessage(
            request.NewPassword);

        if (passwordValidationMessage is not null)
            return ResetPasswordResultDto.Failure(passwordValidationMessage);

        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
            return ResetPasswordResultDto.Failure(
                "No existe un usuario con ese correo.");

        if (!user.IsActive || user.IsDeleted)
            return ResetPasswordResultDto.Failure(
                "El usuario se encuentra inactivo.");

        if (user.PasswordResetTokenHash is null ||
            user.PasswordResetTokenExpiresAt is null)
        {
            return ResetPasswordResultDto.Failure(
                "No existe una solicitud de recuperación activa.");
        }

        if (user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            return ResetPasswordResultDto.Failure(
                "El token de recuperación expiró.");

        var tokenHash = UserService.Hash(request.ResetToken);

        if (tokenHash != user.PasswordResetTokenHash)
            return ResetPasswordResultDto.Failure(
                "El token de recuperación no es válido.");

        var newPasswordHash = UserService.Hash(request.NewPassword);

        if (newPasswordHash == user.PasswordHash)
            return ResetPasswordResultDto.Failure(
                "No puede usar la contraseña actual.");

        var previousHashes = await _userRepository.GetPasswordHistoryHashesAsync(user.Id);

        if (previousHashes.Contains(newPasswordHash))
            return ResetPasswordResultDto.Failure(
                "No puede usar una contraseña anterior.");

        await _userRepository.AddPasswordHistoryAsync(
            new PasswordHistory(user.Id, user.PasswordHash)
        );

        user.ChangePassword(newPasswordHash);
        user.ClearPasswordResetToken();

        await _userRepository.UpdateAsync(user);

        return ResetPasswordResultDto.Success();
    }

    private static string? GetPasswordValidationMessage(string password)
    {
        if (password.Length < 8 || password.Length > 10)
            return "La contraseña debe tener mínimo 8 y máximo 10 caracteres.";

        if (!Regex.IsMatch(password, "[A-Z]"))
            return "La contraseña debe tener al menos una mayúscula.";

        if (!Regex.IsMatch(password, "[a-z]"))
            return "La contraseña debe tener al menos una minúscula.";

        if (!Regex.IsMatch(password, "[0-9]"))
            return "La contraseña debe tener al menos un número.";

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            return "La contraseña debe tener al menos un carácter especial.";

        return null;
    }
}
