using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.DTOs.Customers;

namespace SalesPoint.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<PagedResponse<CustomerDto>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize,
        bool onlyActive);

    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request);
    Task UpdateAsync(int id, UpdateCustomerRequest request);
    Task<DeleteResultDto> DeleteAsync(int id);
    Task<DeleteResultDto> DeactivateAsync(int id);
    Task<DeleteResultDto> ActivateAsync(int id);
}
