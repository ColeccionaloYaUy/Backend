using ColeccionaloYa.Domain.Carts.Exceptions;
using ColeccionaloYa.Persistence.Carts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Carts.RemoveCartItem;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, CartDto> {
	private readonly ICartRepository _CartRepository;

	public RemoveCartItemCommandHandler(ICartRepository cartRepository) {
		_CartRepository = cartRepository;
	}

	public async Task<CartDto> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken) {
		var cartId = await _CartRepository.GetCartIdAsync(request.ClientId, cancellationToken);
		if (cartId is null || !await _CartRepository.ItemExistsAsync(cartId.Value, request.ProductId, cancellationToken)) {
			throw new CartItemNotFoundException(request.ProductId);
		}

		await _CartRepository.RemoveItemAsync(cartId.Value, request.ProductId, cancellationToken);
		await _CartRepository.TouchAsync(cartId.Value, cancellationToken);

		var data = await _CartRepository.GetCartDataAsync(request.ClientId, cancellationToken);
		return CartDto.From(data!);
	}
}
