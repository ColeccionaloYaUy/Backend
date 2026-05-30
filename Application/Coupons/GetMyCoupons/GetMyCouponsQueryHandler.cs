using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.GetMyCoupons;

public class GetMyCouponsQueryHandler : IRequestHandler<GetMyCouponsQuery, List<CouponMeDto>> {
	private readonly ICouponRepository _Repository;

	public GetMyCouponsQueryHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<List<CouponMeDto>> Handle(GetMyCouponsQuery request, CancellationToken cancellationToken) {
		var coupons = await _Repository.GetForClientAsync(request.ClientId, cancellationToken);
		return coupons.Select(CouponMeDto.From).ToList();
	}
}
