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
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IExternalIdentityValidator _externalIdentityValidator;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IExternalIdentityValidator externalIdentityValidator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _externalIdentityValidator = externalIdentityValidator;
    }

    public ExternalAuthenticationStatusDto GetExternalAuthenticationStatus() =>
        new()
        {
            GoogleEnabled = _externalIdentityValidator.IsConfigured,
            GoogleClientId = _externalIdentityValidator.IsConfigured
                ? _externalIdentityValidator.ClientId
                : string.Empty
        };

    public async Task<LoginResultDto> LoginWithGoogleAsync(
        GoogleLoginRequestDto request)
    {
        if (!_externalIdentityValidator.IsConfigured)
            return LoginResultDto.Failure(
                "El acceso con Google no está configurado.");
        if (request is null || string.IsNullOrWhiteSpace(request.Credential))
            return LoginResultDto.Failure(
                "La credencial de Google es obligatoria.");

        var identity = await _externalIdentityValidator
            .ValidateGoogleTokenAsync(request.Credential);

        if (identity is null || !identity.EmailVerified ||
            string.IsNullOrWhiteSpace(identity.Email))
        {
            return LoginResultDto.Failure(
                "Google no pudo verificar la identidad o el correo.");
        }

        var user = await _userRepository.GetByEmailAsync(identity.Email);

        if (user is null)
            return LoginResultDto.Failure(
                "El correo de Google no está registrado en SalesPoint.");
        if (!user.IsActive || user.IsDeleted)
            return LoginResultDto.Failure("El usuario se encuentra inactivo.");
        if (user.IsLocked)
            return LoginResultDto.Failure(
                "El usuario se encuentra bloqueado. Contacte al administrador.");

        return LoginResultDto.Success(await CreateSessionAsync(user));
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        if (request is null)
            return LoginResultDto.Failure("Los datos de inicio de sesión son obligatorios.");
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail))
            return LoginResultDto.Failure("Ingrese su usuario o correo.");
        if (string.IsNullOrWhiteSpace(request.Password))
            return LoginResultDto.Failure("Ingrese su contraseña.");

        var user = await _userRepository
            .GetByUserNameOrEmailAsync(request.UserNameOrEmail);

        if (user is null)
            return LoginResultDto.Failure("Correo o contraseña incorrectos.");
        if (!user.IsActive || user.IsDeleted)
            return LoginResultDto.Failure("El usuario se encuentra inactivo.");
        if (user.IsLocked)
            return LoginResultDto.Failure(
                "El usuario se encuentra bloqueado. Contacte al administrador.");

        if (user.PasswordHash != UserService.Hash(request.Password))
        {
            user.RegisterFailedLoginAttempt();
            await _userRepository.UpdateAsync(user);

            if (user.IsLocked)
                return LoginResultDto.Failure(
                    "El usuario se encuentra bloqueado. Contacte al administrador.");

            return LoginResultDto.Failure(
                $"Correo o contraseña incorrectos. Intentos restantes: {3 - user.FailedLoginAttempts}.");
        }

        if (user.FailedLoginAttempts > 0)
        {
            user.ResetFailedLoginAttempts();
            await _userRepository.UpdateAsync(user);
        }

        return LoginResultDto.Success(await CreateSessionAsync(user));
    }

    public async Task<LoginResultDto> RefreshAsync(RefreshTokenRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return LoginResultDto.Failure("El token de renovación es obligatorio.");

        var storedToken = await _refreshTokenRepository
            .GetByHashAsync(UserService.Hash(request.RefreshToken));
        var user = storedToken?.User;

        if (storedToken is null || !storedToken.IsValid ||
            user is null || !user.IsActive || user.IsDeleted || user.IsLocked)
        {
            return LoginResultDto.Failure("Su sesión expiró. Ingrese nuevamente.");
        }

        var newRawToken = GenerateSecureToken();
        storedToken.Revoke(UserService.Hash(newRawToken));
        await _refreshTokenRepository.SaveChangesAsync();

        return LoginResultDto.Success(await CreateSessionAsync(user, newRawToken));
    }

    public async Task LogoutAsync(LogoutRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return;

        var storedToken = await _refreshTokenRepository
            .GetByHashAsync(UserService.Hash(request.RefreshToken));

        if (storedToken is null)
            return;

        storedToken.Revoke();
        await _refreshTokenRepository.SaveChangesAsync();
    }

    public async Task<ForgotPasswordResultDto> ForgotPasswordAsync(
        ForgotPasswordRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
            return ForgotPasswordResultDto.Failure("Ingrese su correo o usuario.");

        var user = await _userRepository.GetByUserNameOrEmailAsync(request.Email);

        // Password Recovery: respuesta genérica para no revelar cuentas existentes.
        if (user is null || !user.IsActive || user.IsDeleted)
            return ForgotPasswordResultDto.Success(new ForgotPasswordResponseDto());

        var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.SetPasswordResetToken(
            UserService.Hash(resetToken),
            DateTime.UtcNow.AddMinutes(15));
        await _userRepository.UpdateAsync(user);

        return ForgotPasswordResultDto.Success(
            new ForgotPasswordResponseDto { ResetToken = resetToken });
    }

    public async Task<ResetPasswordResultDto> ResetPasswordAsync(
        ResetPasswordRequestDto request)
    {
        if (request is null)
            return ResetPasswordResultDto.Failure(
                "Los datos para cambiar la contraseña son obligatorios.");
        if (string.IsNullOrWhiteSpace(request.Email))
            return ResetPasswordResultDto.Failure("Ingrese su correo.");
        if (string.IsNullOrWhiteSpace(request.ResetToken))
            return ResetPasswordResultDto.Failure("Ingrese el token de recuperación.");
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return ResetPasswordResultDto.Failure("Ingrese la nueva contraseña.");
        if (request.NewPassword != request.ConfirmPassword)
            return ResetPasswordResultDto.Failure("Las contraseñas no coinciden.");

        var passwordValidationMessage = GetPasswordValidationMessage(request.NewPassword);
        if (passwordValidationMessage is not null)
            return ResetPasswordResultDto.Failure(passwordValidationMessage);

        var user = await _userRepository.GetByEmailAsync(request.Email);
        const string invalidTokenMessage =
            "El token de recuperación no es válido o expiró.";

        if (user is null || !user.IsActive || user.IsDeleted ||
            user.PasswordResetTokenHash is null ||
            user.PasswordResetTokenExpiresAt is null ||
            user.PasswordResetTokenExpiresAt < DateTime.UtcNow ||
            UserService.Hash(request.ResetToken) != user.PasswordResetTokenHash)
        {
            return ResetPasswordResultDto.Failure(invalidTokenMessage);
        }

        var newPasswordHash = UserService.Hash(request.NewPassword);
        if (newPasswordHash == user.PasswordHash)
            return ResetPasswordResultDto.Failure("No puede usar la contraseña actual.");

        var previousHashes = await _userRepository
            .GetPasswordHistoryHashesAsync(user.Id);
        if (previousHashes.Contains(newPasswordHash))
            return ResetPasswordResultDto.Failure(
                "No puede usar una contraseña anterior.");

        await _userRepository.AddPasswordHistoryAsync(
            new PasswordHistory(user.Id, user.PasswordHash));
        user.ChangePassword(newPasswordHash);
        user.ClearPasswordResetToken();
        await _userRepository.UpdateAsync(user);

        return ResetPasswordResultDto.Success();
    }

    private async Task<LoginResponseDto> CreateSessionAsync(
        User user,
        string? refreshToken = null)
    {
        var roleName = user.Role?.Name ?? string.Empty;
        var accessToken = _jwtTokenGenerator.Generate(
            user,
            roleName,
            out var expiration);
        var rawRefreshToken = refreshToken ?? GenerateSecureToken();

        await _refreshTokenRepository.AddAsync(new RefreshToken(
            user.Id,
            UserService.Hash(rawRefreshToken),
            DateTime.UtcNow.AddDays(7)));

        return new LoginResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            RoleName = roleName,
            Token = accessToken,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            Expiration = expiration,
            Role = roleName,
            User = new UserSessionDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            }
        };
    }

    private static string GenerateSecureToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public static string? GetPasswordValidationMessage(string password)
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
