using SalesPoint.Application.DTOs.Roles;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class RoleService : IRoleService
{
    private static readonly string[] SystemRoles =
    [
        "ADMINISTRATOR",
        "SELLER"
    ];

    private readonly IRoleRepository _repository;

    public RoleService(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        return (await _repository.GetAllAsync()).Select(Map).ToList();
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request)
    {
        if (request is null)
            throw new DomainException("Los datos del rol son obligatorios.");

        ApplicationValidator.Required(request.Name, "El rol");

        if (await _repository.ExistsByNameAsync(request.Name))
            throw new DomainException("Ya existe un rol con ese nombre.");

        var role = new Role(request.Name, request.Description);
        await _repository.CreateAsync(role);
        return Map(role);
    }

    public async Task UpdateAsync(int id, UpdateRoleRequest request)
    {
        if (request is null)
            throw new DomainException("Los datos del rol son obligatorios.");

        var role = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Rol no encontrado.");

        ApplicationValidator.Required(request.Name, "El rol");

        if (await _repository.ExistsByNameAsync(request.Name, id))
            throw new DomainException("Ya existe otro rol con ese nombre.");

        var requestedName = request.Name.Trim().ToUpperInvariant();
        var isSystemRole = SystemRoles.Contains(
            role.Name,
            StringComparer.OrdinalIgnoreCase);

        if (isSystemRole &&
            !string.Equals(
                role.Name,
                requestedName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(
                "Los roles ADMINISTRATOR y SELLER no pueden cambiar de nombre.");
        }

        if (isSystemRole && !request.IsActive)
        {
            throw new DomainException(
                "Los roles ADMINISTRATOR y SELLER no pueden desactivarse.");
        }

        role.Update(request.Name, request.Description);

        if (request.IsActive)
            role.Activate();
        else
            role.Deactivate();

        await _repository.UpdateAsync(role);
    }

    private static RoleDto Map(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive
        };
    }
}
