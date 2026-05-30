using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Domain.Stock;
using ColeccionaloYa.Domain.Stock.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Stock.CreateStockMovement;

public class CreateStockMovementCommandHandler : IRequestHandler<CreateStockMovementCommand, StockMovementCreatedDto> {
	private readonly IStockRepository _Repository;
	private readonly IProductRepository _ProductRepository;

	public CreateStockMovementCommandHandler(IStockRepository repository, IProductRepository productRepository) {
		_Repository = repository;
		_ProductRepository = productRepository;
	}

	public async Task<StockMovementCreatedDto> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken) {
		if (!Enum.TryParse<StockMovementType>(request.Type, ignoreCase: true, out var type) || !Enum.IsDefined(type)) {
			throw new InvalidStockTypeException(request.Type);
		}

		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		if (request.OrderId.HasValue && !await _Repository.OrderExistsAsync(request.OrderId.Value, cancellationToken)) {
			throw new OrderNotFoundException(request.OrderId.Value);
		}

		var movement = StockMovement.Create(request.ProductId, type, request.Quantity, request.Date, request.OrderId, request.Reason);

		var currentStock = await _Repository.GetCurrentStockAsync(request.ProductId, cancellationToken);
		movement.EnsureDoesNotCauseNegativeStock(currentStock);

		await _Repository.CreateAsync(movement, cancellationToken);
		return StockMovementCreatedDto.From(movement);
	}
}
