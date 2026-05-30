using ColeccionaloYa.Domain.Coupons;
using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.CreateCoupon;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponDto> {
	private readonly ICouponRepository _Repository;

	public CreateCouponCommandHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken) {
		if (await _Repository.ExistsByTokenAsync(request.Token, null, cancellationToken)) {
			throw new DuplicateCouponTokenException(request.Token);
		}

		var coupon = Coupon.Create(request.Name, request.Token, request.Description, request.ValidFrom, request.ValidUntil, request.Porcentage);
		await _Repository.CreateAsync(coupon, cancellationToken);
		return CouponDto.From(coupon);
	}
}
