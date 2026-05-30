using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.ActivateCoupon;

public class ActivateCouponCommandHandler : IRequestHandler<ActivateCouponCommand, CouponDto> {
	private readonly ICouponRepository _Repository;

	public ActivateCouponCommandHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<CouponDto> Handle(ActivateCouponCommand request, CancellationToken cancellationToken) {
		var coupon = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new CouponNotFoundException(request.Id);

		coupon.Activate();
		await _Repository.UpdateAsync(coupon, cancellationToken);
		return CouponDto.From(coupon);
	}
}
