using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.DTOs.Products;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<ProductDto>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 8 : pageSize;

        var data = await _repository.SearchAsync(field, value, pageNumber, pageSize);
        var totalItems = await _repository.CountAsync(field, value);

        var items = data.Select(Map).ToList();

        return new PagedResponse<ProductDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? null : Map(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        ApplicationValidator.Required(request.Name, "El nombre del producto");
        ApplicationValidator.Positive(request.Price, "El precio");
        ApplicationValidator.NotNegative(request.Stock, "El stock");

        if (await _repository.ExistsByNameAsync(request.Name))
            throw new DomainException("Ya existe un producto con el mismo nombre.");

        var product = new Product(
            request.Name,
            request.Price,
            request.Stock
        );

        await _repository.CreateAsync(product);

        return Map(product);
    }

    public async Task UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Producto no encontrado.");

        ApplicationValidator.Required(request.Name, "El nombre del producto");
        ApplicationValidator.Positive(request.Price, "El precio");
        ApplicationValidator.NotNegative(request.Stock, "El stock");

        if (await _repository.ExistsByNameAsync(request.Name, id))
            throw new DomainException("Ya existe otro producto con el mismo nombre.");

        product.Update(
            request.Name,
            request.Price,
            request.Stock
        );

        await _repository.UpdateAsync(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("Producto no encontrado.");

        product.Deactivate();

        await _repository.UpdateAsync(product);
    }

    private static ProductDto Map(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive
        };
    }
}