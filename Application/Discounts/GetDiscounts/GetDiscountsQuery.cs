using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Discounts.GetDiscounts;

public record GetDiscountsQuery(int Page, int PageSize, bool? IsActive, int? ProductId)
	: IRequest<PagedResult<DiscountDto>>, IPagedQuery;
