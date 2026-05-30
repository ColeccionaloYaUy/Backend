using ColeccionaloYa.Domain.Carts.Exceptions;
using ColeccionaloYa.Persistence.Carts.Interfaces;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Carts.UpdateCartItem;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartDto> {
	private readonly ICartRepository _CartRepository;
	private readonly IStockRepository _StockRepository;

	public UpdateCartItemCommandHandler(ICartRepository cartRepository, IStockRepository stockRepository) {
		_CartRepository = cartRepository;
		_StockRepository = stockRepository;
	}

	public async Task<CartDto> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken) {
		if (request.Quantity <= 0) {
			throw new InvalidCartQuantityException();
		}

		var cartId = await _CartRepository.GetCartIdAsync(request.ClientId, cancellationToken);
		if (cartId is null || !await _CartRepository.ItemExistsAsync(cartId.Value, request.ProductId, cancellationToken)) {
			throw new CartItemNotFoundException(request.ProductId);
		}

		var currentStock = await _StockRepository.GetCurrentStockAsync(request.ProductId, cancellationToken);
		if (request.Quantity > currentStock) {
			throw new InsufficientStockException(request.ProductId);
		}

		await _CartRepository.SetItemQuantityAsync(cartId.Value, request.ProductId, request.Quantity, cancellationToken);
		await _CartRepository.TouchAsync(cartId.Value, cancellationToken);

		var data = await _CartRepository.GetCartDataAsync(request.ClientId, cancellationToken);
		return CartDto.From(data!);
	}
}
