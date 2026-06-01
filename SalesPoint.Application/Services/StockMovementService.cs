using SalesPoint.Application.DTOs.StockMovements;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Validators;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class StockMovementService : IStockMovementService
{
    private readonly IStockMovementRepository _repository;
    private readonly IProductRepository _productRepository;

    public StockMovementService(
        IStockMovementRepository repository,
        IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<List<StockMovementDto>> GetAllAsync()
    {
        return (await _repository.GetAllAsync())
            .Select(Map)
            .ToList();
    }

    public async Task<List<StockMovementDto>> GetByProductIdAsync(int productId)
    {
        return (await _repository.GetByProductIdAsync(productId))
            .Select(Map)
            .ToList();
    }

    public async Task<StockMovementDto> CreateAsync(CreateStockMovementRequest request)
    {
        ApplicationValidator.Positive(request.ProductId, "El producto");
        ApplicationValidator.Positive(request.Quantity, "La cantidad");

        var product = await _productRepository.GetByIdAsync(request.ProductId)
            ?? throw new DomainException("Producto no encontrado.");

        var movement = new StockMovement(
            product.Id,
            request.MovementType,
            request.Quantity,
            product.Stock,
            request.Reason
        );

        await _repository.CreateAsync(movement);

        return Map(movement);
    }

    private static StockMovementDto Map(StockMovement movement)
    {
        return new StockMovementDto
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            ProductName = movement.Product?.Name ?? string.Empty,
            Quantity = movement.Quantity,
            MovementType = movement.MovementType,
            Reason = movement.Reason,
            CreatedAt = movement.CreatedAt
        };
    }
}