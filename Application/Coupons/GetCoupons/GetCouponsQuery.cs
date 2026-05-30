using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.GetCoupons;

public record GetCouponsQuery(int Page, int PageSize, bool? IsActive, string? Search)
	: IRequest<PagedResult<CouponListItemDto>>, IPagedQuery;
