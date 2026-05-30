using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Carts.Interfaces;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.ValidateCoupon;

public class ValidateCouponCommandHandler : IRequestHandler<ValidateCouponCommand, CouponValidateDto> {
	private readonly ICouponRepository _Repository;
	private readonly ICartRepository _CartRepository;

	public ValidateCouponCommandHandler(ICouponRepository repository, ICartRepository cartRepository) {
		_Repository = repository;
		_CartRepository = cartRepository;
	}

	public async Task<CouponValidateDto> Handle(ValidateCouponCommand request, CancellationToken cancellationToken) {
		var coupon = await _Repository.GetByTokenAsync(request.Token, cancellationToken)
			?? throw new CouponTokenNotFoundException(request.Token);

		var now = DateTime.UtcNow;
		string? reason = null;
		if (!coupon.IsActive) {
			reason = "inactive";
		} else if (!coupon.IsWithinWindow(now)) {
			reason = "expired";
		} else if (!await _Repository.IsAssignedAsync(coupon.Id, request.ClientId, cancellationToken)) {
			reason = "not_assigned";
		}

		if (reason is not null) {
			return new CouponValidateDto(false, reason, null, 0m);
		}

		var subtotal = await _CartRepository.GetNetSubtotalAsync(request.ClientId, cancellationToken);
		var discount = Math.Round(subtotal * coupon.Porcentage / 100m, 2);

		return new CouponValidateDto(true, null, CouponValidateCouponDto.From(coupon), discount);
	}
}
