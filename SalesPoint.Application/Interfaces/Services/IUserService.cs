using SalesPoint.Application.DTOs.Users;
namespace SalesPoint.Application.Interfaces.Services;
public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task UpdateAsync(int id, UpdateUserRequest request);
    Task DisableAsync(int id);
    Task ActivateAsync(int id);
    Task UnlockAsync(int id);
}
