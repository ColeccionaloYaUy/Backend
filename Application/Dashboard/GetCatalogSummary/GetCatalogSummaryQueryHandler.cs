using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetCatalogSummary;

public class GetCatalogSummaryQueryHandler : IRequestHandler<GetCatalogSummaryQuery, CatalogSummaryDto> {
	private readonly IDashboardRepository _Repository;

	public GetCatalogSummaryQueryHandler(IDashboardRepository repository) {
		_Repository = repository;
	}

	public async Task<CatalogSummaryDto> Handle(GetCatalogSummaryQuery request, CancellationToken cancellationToken) {
		var row = await _Repository.GetCatalogSummaryAsync(cancellationToken);
		return CatalogSummaryDto.From(row);
	}
}
