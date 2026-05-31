using SalesPoint.Application.DTOs.Roles;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;
public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _repository;
    public RoleService(IRoleRepository repository) => _repository = repository;
    public async Task<List<RoleDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(Map).ToList();
    public async Task<RoleDto> CreateAsync(CreateRoleRequest request)
    {
        ApplicationValidator.Required(request.Name, "El rol");
        if (await _repository.ExistsByNameAsync(request.Name)) throw new DomainException("Ya existe un rol con ese nombre.");
        var role = new Role(request.Name, request.Description);
        await _repository.CreateAsync(role);
        return Map(role);
    }
    private static RoleDto Map(Role role) => new() { Id = role.Id, Name = role.Name, Description = role.Description, IsActive = role.IsActive };
}
