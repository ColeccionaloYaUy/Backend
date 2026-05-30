using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.GetCoupons;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, PagedResult<CouponListItemDto>> {
	private readonly ICouponRepository _Repository;

	public GetCouponsQueryHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<CouponListItemDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetPagedAsync(request.Page, request.PageSize, request.IsActive, request.Search, cancellationToken);
		return data.ToPagedResult(request.PageSize, CouponListItemDto.From);
	}
}
