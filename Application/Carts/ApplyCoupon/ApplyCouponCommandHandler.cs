using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Carts.Interfaces;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Carts.ApplyCoupon;

public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, ApplyCouponResultDto> {
	private readonly ICartRepository _CartRepository;
	private readonly ICouponRepository _CouponRepository;

	public ApplyCouponCommandHandler(ICartRepository cartRepository, ICouponRepository couponRepository) {
		_CartRepository = cartRepository;
		_CouponRepository = couponRepository;
	}

	public async Task<ApplyCouponResultDto> Handle(ApplyCouponCommand request, CancellationToken cancellationToken) {
		var coupon = await _CouponRepository.GetByTokenAsync(request.CouponToken, cancellationToken)
			?? throw new CouponTokenNotFoundException(request.CouponToken);

		var now = DateTime.UtcNow;
		var usable = coupon.IsActive
			&& coupon.IsWithinWindow(now)
			&& await _CouponRepository.IsAssignedAsync(coupon.Id, request.ClientId, cancellationToken);

		if (!usable) {
			throw new CouponNotUsableException();
		}

		var cartId = await _CartRepository.EnsureCartAsync(request.ClientId, cancellationToken);
		await _CartRepository.SetCouponAsync(cartId, coupon.Id, cancellationToken);

		var data = await _CartRepository.GetCartDataAsync(request.ClientId, cancellationToken);
		var dto = CartDto.From(data!);
		var discountApplied = CartDto.CouponDiscount(data!);
		return new ApplyCouponResultDto(dto, discountApplied);
	}
}
