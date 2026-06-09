using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.DTOs.Products;

namespace SalesPoint.Application.Interfaces.Services;

public interface IProductService
{
    Task<PagedResponse<ProductDto>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize,
        bool onlyAvailable);

    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task UpdateAsync(int id, UpdateProductRequest request);
    Task<DeleteResultDto> DeleteAsync(int id);
    Task<DeleteResultDto> DeactivateAsync(int id);
    Task<DeleteResultDto> ActivateAsync(int id);
}
