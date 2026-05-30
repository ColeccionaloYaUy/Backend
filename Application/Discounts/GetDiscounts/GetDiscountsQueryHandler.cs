using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Persistence.Discounts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Discounts.GetDiscounts;

public class GetDiscountsQueryHandler : IRequestHandler<GetDiscountsQuery, PagedResult<DiscountDto>> {
	private readonly IDiscountRepository _Repository;

	public GetDiscountsQueryHandler(IDiscountRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<DiscountDto>> Handle(GetDiscountsQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetPagedAsync(request.Page, request.PageSize, request.IsActive, request.ProductId, cancellationToken);
		return data.ToPagedResult(request.PageSize, DiscountDto.From);
	}
}
