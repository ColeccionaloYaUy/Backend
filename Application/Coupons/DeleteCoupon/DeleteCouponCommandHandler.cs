using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.DeleteCoupon;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, Unit> {
	private readonly ICouponRepository _Repository;

	public DeleteCouponCommandHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteCouponCommand request, CancellationToken cancellationToken) {
		var coupon = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new CouponNotFoundException(request.Id);

		await _Repository.LogicalDeleteAsync(coupon.Id, cancellationToken);
		return Unit.Value;
	}
}
