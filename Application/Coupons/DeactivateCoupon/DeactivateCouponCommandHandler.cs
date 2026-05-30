using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.DeactivateCoupon;

public class DeactivateCouponCommandHandler : IRequestHandler<DeactivateCouponCommand, CouponDto> {
	private readonly ICouponRepository _Repository;

	public DeactivateCouponCommandHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<CouponDto> Handle(DeactivateCouponCommand request, CancellationToken cancellationToken) {
		var coupon = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new CouponNotFoundException(request.Id);

		coupon.Deactivate();
		await _Repository.UpdateAsync(coupon, cancellationToken);
		return CouponDto.From(coupon);
	}
}
