using ColeccionaloYa.Persistence.Carts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Carts.ClearCart;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Unit> {
	private readonly ICartRepository _CartRepository;

	public ClearCartCommandHandler(ICartRepository cartRepository) {
		_CartRepository = cartRepository;
	}

	public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken) {
		var cartId = await _CartRepository.GetCartIdAsync(request.ClientId, cancellationToken);
		if (cartId is not null) {
			await _CartRepository.ClearItemsAsync(cartId.Value, cancellationToken);
			await _CartRepository.TouchAsync(cartId.Value, cancellationToken);
		}

		return Unit.Value;
	}
}
