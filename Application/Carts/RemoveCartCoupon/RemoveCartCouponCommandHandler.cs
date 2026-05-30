using ColeccionaloYa.Domain.Carts.Exceptions;
using ColeccionaloYa.Persistence.Carts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Carts.RemoveCartCoupon;

public class RemoveCartCouponCommandHandler : IRequestHandler<RemoveCartCouponCommand, CartDto> {
	private readonly ICartRepository _CartRepository;

	public RemoveCartCouponCommandHandler(ICartRepository cartRepository) {
		_CartRepository = cartRepository;
	}

	public async Task<CartDto> Handle(RemoveCartCouponCommand request, CancellationToken cancellationToken) {
		var data = await _CartRepository.GetCartDataAsync(request.ClientId, cancellationToken);
		if (data is null || data.IdCoupon is null) {
			throw new CartCouponNotAppliedException();
		}

		await _CartRepository.SetCouponAsync(data.IdCart, null, cancellationToken);

		var updated = await _CartRepository.GetCartDataAsync(request.ClientId, cancellationToken);
		return CartDto.From(updated!);
	}
}
