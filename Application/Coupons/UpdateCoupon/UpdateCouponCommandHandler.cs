using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.UpdateCoupon;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, CouponDto> {
	private readonly ICouponRepository _Repository;

	public UpdateCouponCommandHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<CouponDto> Handle(UpdateCouponCommand request, CancellationToken cancellationToken) {
		var coupon = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new CouponNotFoundException(request.Id);

		coupon.Update(request.Name, request.Description, request.ValidFrom, request.ValidUntil, request.Porcentage);
		await _Repository.UpdateAsync(coupon, cancellationToken);
		return CouponDto.From(coupon);
	}
}
