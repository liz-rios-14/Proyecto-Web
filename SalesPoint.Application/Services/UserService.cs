using System.Security.Cryptography;
using System.Text;
using SalesPoint.Application.DTOs.Users;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    public UserService(IUserRepository userRepository, IRoleRepository roleRepository) { _userRepository = userRepository; _roleRepository = roleRepository; }
    public async Task<List<UserDto>> GetAllAsync() => (await _userRepository.GetAllAsync()).Select(Map).ToList();
    public async Task<UserDto?> GetByIdAsync(int id) => (await _userRepository.GetByIdAsync(id)) is { } user ? Map(user) : null;
    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        ApplicationValidator.Required(request.UserName, "El usuario");
        ApplicationValidator.Email(request.Email);
        ApplicationValidator.Required(request.Password, "La contraseña");
        if (request.Password.Length < 6) throw new DomainException("La contraseña debe tener al menos 6 caracteres.");
        if (await _roleRepository.GetByIdAsync(request.RoleId) is null) throw new DomainException("El rol seleccionado no existe.");
        if (await _userRepository.ExistsByUserNameAsync(request.UserName)) throw new DomainException("Ya existe un usuario con ese nombre.");
        if (await _userRepository.ExistsByEmailAsync(request.Email)) throw new DomainException("Ya existe un usuario con ese correo.");
        var user = new User(request.UserName, request.Email, Hash(request.Password), request.RoleId);
        await _userRepository.CreateAsync(user);
        return Map(user);
    }
    public async Task UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id) ?? throw new DomainException("Usuario no encontrado.");
        if (await _roleRepository.GetByIdAsync(request.RoleId) is null) throw new DomainException("El rol seleccionado no existe.");
        if (await _userRepository.ExistsByUserNameAsync(request.UserName, id)) throw new DomainException("Ya existe otro usuario con ese nombre.");
        if (await _userRepository.ExistsByEmailAsync(request.Email, id)) throw new DomainException("Ya existe otro usuario con ese correo.");
        user.Update(request.UserName, request.Email, request.RoleId, request.IsActive);
        await _userRepository.UpdateAsync(user);
    }
    public async Task DisableAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id) ?? throw new DomainException("Usuario no encontrado.");
        user.Disable();
        await _userRepository.UpdateAsync(user);
    }
    internal static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static UserDto Map(User user) => new() { Id = user.Id, UserName = user.UserName, Email = user.Email, RoleId = user.RoleId, RoleName = user.Role?.Name ?? string.Empty, IsActive = user.IsActive };
}
