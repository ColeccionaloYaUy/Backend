using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.GetCouponById;

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, CouponDetailDto> {
	private readonly ICouponRepository _Repository;

	public GetCouponByIdQueryHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<CouponDetailDto> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetWithClientsAsync(request.Id, cancellationToken)
			?? throw new CouponNotFoundException(request.Id);
		return CouponDetailDto.From(data);
	}
}
