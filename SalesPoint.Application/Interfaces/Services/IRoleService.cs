using SalesPoint.Application.DTOs.Roles;
namespace SalesPoint.Application.Interfaces.Services;
public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync();
    Task<RoleDto> CreateAsync(CreateRoleRequest request);
}
