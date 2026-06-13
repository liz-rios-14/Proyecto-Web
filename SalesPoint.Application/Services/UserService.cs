using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SalesPoint.Application.DTOs.Users;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class UserService : IUserService
{
    private const string AdministratorRole = "ADMINISTRATOR";
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return (await _userRepository.GetAllAsync()).Select(Map).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        return (await _userRepository.GetByIdAsync(id)) is { } user
            ? Map(user)
            : null;
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        if (request is null)
            throw new DomainException("Los datos del usuario son obligatorios.");

        ValidateUserData(
            request.FullName,
            request.UserName,
            request.Email);
        ApplicationValidator.Required(request.Password, "La contraseña");
        ValidatePasswordPolicy(request.Password);

        var role = await _roleRepository.GetByIdAsync(request.RoleId)
            ?? throw new DomainException("El rol seleccionado no existe.");

        if (!role.IsActive)
            throw new DomainException("El rol seleccionado está inactivo.");

        if (await _userRepository.ExistsByUserNameAsync(request.UserName))
            throw new DomainException("Ya existe un usuario con ese nombre.");

        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new DomainException("Ya existe un usuario con ese correo.");

        var user = new User(
            request.RoleId,
            request.FullName,
            request.UserName,
            request.Email,
            Hash(request.Password));

        await _userRepository.CreateAsync(user);
        return Map(user);
    }

    public async Task UpdateAsync(int id, UpdateUserRequest request)
    {
        if (request is null)
            throw new DomainException("Los datos del usuario son obligatorios.");

        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new DomainException("Usuario no encontrado.");

        ValidateUserData(
            request.FullName,
            request.UserName,
            request.Email);

        var role = await _roleRepository.GetByIdAsync(request.RoleId)
            ?? throw new DomainException("El rol seleccionado no existe.");

        if (!role.IsActive)
            throw new DomainException("El rol seleccionado está inactivo.");

        if (await _userRepository.ExistsByUserNameAsync(request.UserName, id))
            throw new DomainException("Ya existe otro usuario con ese nombre.");

        if (await _userRepository.ExistsByEmailAsync(request.Email, id))
            throw new DomainException("Ya existe otro usuario con ese correo.");

        EnsureAdministratorRemainsActive(user, request.IsActive);

        user.Update(
            request.FullName,
            request.UserName,
            request.Email,
            request.RoleId,
            request.IsActive);

        await _userRepository.UpdateAsync(user);
    }

    public async Task DisableAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new DomainException("Usuario no encontrado.");

        EnsureAdministratorRemainsActive(user, false);
        user.Disable();
        await _userRepository.UpdateAsync(user);
    }

    public async Task ActivateAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new DomainException("Usuario no encontrado.");

        if (user.IsActive)
            throw new DomainException("El usuario ya se encuentra activo.");

        user.Activate();
        await _userRepository.UpdateAsync(user);
    }

    public async Task UnlockAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new DomainException("Usuario no encontrado.");

        if (!user.IsLocked)
            throw new DomainException("El usuario no se encuentra bloqueado.");

        if (!user.IsActive || user.IsDeleted)
        {
            throw new DomainException(
                "El usuario está inactivo o eliminado. Actívelo antes de desbloquearlo.");
        }

        user.Unlock();
        await _userRepository.UpdateAsync(user);
    }

    private static void EnsureAdministratorRemainsActive(
        User user,
        bool newActiveState)
    {
        if (user.IsActive &&
            !newActiveState &&
            string.Equals(
                user.Role?.Name,
                AdministratorRole,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(
                "Los usuarios administradores no pueden desactivarse desde este módulo.");
        }
    }

    internal static string Hash(string value)
    {
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static void ValidateUserData(
        string fullName,
        string userName,
        string email)
    {
        ApplicationValidator.Required(fullName, "El nombre completo");
        ApplicationValidator.Required(userName, "El usuario");
        ApplicationValidator.Email(email);

        if (fullName.Trim().Length < 3 || fullName.Trim().Length > 120)
            throw new DomainException("El nombre completo debe tener entre 3 y 120 caracteres.");

        if (userName.Trim().Length < 3 || userName.Trim().Length > 60)
            throw new DomainException("El usuario debe tener entre 3 y 60 caracteres.");

        if (!Regex.IsMatch(fullName.Trim(), @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+(?: [A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+)*$"))
            throw new DomainException("Los nombres y apellidos solo pueden contener letras y espacios simples.");

        if (!Regex.IsMatch(userName.Trim(), @"^[A-Za-z0-9._-]+$"))
            throw new DomainException("El usuario no puede contener espacios ni caracteres especiales distintos de punto, guion o guion bajo.");
    }

    private static void ValidatePasswordPolicy(string password)
    {
        if (password.Length < 8 || password.Length > 10)
            throw new DomainException("La contraseña debe tener entre 8 y 10 caracteres.");
        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new DomainException("La contraseña debe incluir una mayúscula.");
        if (!Regex.IsMatch(password, "[a-z]"))
            throw new DomainException("La contraseña debe incluir una minúscula.");
        if (!Regex.IsMatch(password, "[0-9]"))
            throw new DomainException("La contraseña debe incluir un número.");
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            throw new DomainException("La contraseña debe incluir un carácter especial.");
    }

    private static UserDto Map(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            UserName = user.UserName,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            FailedLoginAttempts = user.FailedLoginAttempts,
            IsLocked = user.IsLocked
        };
    }
}
