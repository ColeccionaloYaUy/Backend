using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetTopProducts;

public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, TopProductsDto> {
	private readonly IDashboardRepository _Repository;

	public GetTopProductsQueryHandler(IDashboardRepository repository) {
		_Repository = repository;
	}

	public async Task<TopProductsDto> Handle(GetTopProductsQuery request, CancellationToken cancellationToken) {
		var (from, to) = DashboardDateRange.Resolve(request.DateFrom, request.DateTo);
		var limit = request.Limit <= 0 ? 10 : request.Limit;
		var rows = await _Repository.GetTopProductsAsync(limit, from, to, request.Type, cancellationToken);
		return new TopProductsDto(rows.Select(TopProductDto.From).ToList());
	}
}
