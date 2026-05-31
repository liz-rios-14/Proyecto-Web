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
    public StockMovementService(IStockMovementRepository repository, IProductRepository productRepository) { _repository = repository; _productRepository = productRepository; }
    public async Task<List<StockMovementDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(Map).ToList();
    public async Task<List<StockMovementDto>> GetByProductIdAsync(int productId) => (await _repository.GetByProductIdAsync(productId)).Select(Map).ToList();
    public async Task<StockMovementDto> CreateAsync(CreateStockMovementRequest request)
    {
        ApplicationValidator.Positive(request.ProductId, "El producto");
        ApplicationValidator.Positive(request.Quantity, "La cantidad");
        var product = await _productRepository.GetByIdAsync(request.ProductId) ?? throw new DomainException("Producto no encontrado.");
        var movement = new StockMovement(product.Id, request.Quantity, request.MovementType, request.Reason);
        await _repository.CreateAsync(movement);
        return Map(movement);
    }
    private static StockMovementDto Map(StockMovement m) => new() { Id = m.Id, ProductId = m.ProductId, ProductName = m.Product?.Name ?? string.Empty, Quantity = m.Quantity, MovementType = m.MovementType, Reason = m.Reason, CreatedAt = m.CreatedAt };
}
