using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.RemoveCouponClient;

public class RemoveCouponClientCommandHandler : IRequestHandler<RemoveCouponClientCommand, Unit> {
	private readonly ICouponRepository _Repository;

	public RemoveCouponClientCommandHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(RemoveCouponClientCommand request, CancellationToken cancellationToken) {
		_ = await _Repository.GetByIdAsync(request.CouponId, cancellationToken)
			?? throw new CouponNotFoundException(request.CouponId);

		if (!await _Repository.ClientAssignmentExistsAsync(request.CouponId, request.ClientId, cancellationToken)) {
			throw new CouponClientNotAssignedException(request.CouponId, request.ClientId);
		}

		await _Repository.RemoveClientAsync(request.CouponId, request.ClientId, cancellationToken);
		return Unit.Value;
	}
}
