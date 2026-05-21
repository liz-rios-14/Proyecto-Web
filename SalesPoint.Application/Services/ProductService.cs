using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.DTOs.Products;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
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
        var data = await _repository.SearchAsync(field, value);

        var totalItems = data.Count;

        var items = data
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock
            })
            .ToList();

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

        if (product == null) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var product = new Product(request.Name, request.Price, request.Stock);

        await _repository.CreateAsync(product);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        };
    }

    public async Task UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id);

        if (product == null)
            throw new DomainException("Producto no encontrado.");

        product.Update(request.Name, request.Price, request.Stock);

        await _repository.UpdateAsync(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);

        if (product == null)
            throw new DomainException("Producto no encontrado.");

        await _repository.DeleteAsync(product);
    }
}