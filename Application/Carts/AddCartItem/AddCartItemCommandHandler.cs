using ColeccionaloYa.Domain.Carts.Exceptions;
using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Carts.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Carts.AddCartItem;

public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, CartDto> {
	private readonly ICartRepository _CartRepository;
	private readonly IProductRepository _ProductRepository;
	private readonly IStockRepository _StockRepository;

	public AddCartItemCommandHandler(
		ICartRepository cartRepository,
		IProductRepository productRepository,
		IStockRepository stockRepository) {
		_CartRepository = cartRepository;
		_ProductRepository = productRepository;
		_StockRepository = stockRepository;
	}

	public async Task<CartDto> Handle(AddCartItemCommand request, CancellationToken cancellationToken) {
		if (request.Quantity <= 0) {
			throw new InvalidCartQuantityException();
		}

		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		var cartId = await _CartRepository.EnsureCartAsync(request.ClientId, cancellationToken);
		var existing = await _CartRepository.GetItemQuantityAsync(cartId, request.ProductId, cancellationToken) ?? 0;
		var total = existing + request.Quantity;

		var currentStock = await _StockRepository.GetCurrentStockAsync(request.ProductId, cancellationToken);
		if (total > currentStock) {
			throw new InsufficientStockException(request.ProductId);
		}

		if (existing > 0) {
			await _CartRepository.SetItemQuantityAsync(cartId, request.ProductId, total, cancellationToken);
		} else {
			await _CartRepository.InsertItemAsync(cartId, request.ProductId, request.Quantity, cancellationToken);
		}

		await _CartRepository.TouchAsync(cartId, cancellationToken);

		var data = await _CartRepository.GetCartDataAsync(request.ClientId, cancellationToken);
		return CartDto.From(data!);
	}
}
