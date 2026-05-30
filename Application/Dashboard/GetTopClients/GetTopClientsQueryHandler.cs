using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetTopClients;

public class GetTopClientsQueryHandler : IRequestHandler<GetTopClientsQuery, TopClientsDto> {
	private readonly IDashboardRepository _Repository;

	public GetTopClientsQueryHandler(IDashboardRepository repository) {
		_Repository = repository;
	}

	public async Task<TopClientsDto> Handle(GetTopClientsQuery request, CancellationToken cancellationToken) {
		var (from, to) = DashboardDateRange.Resolve(request.DateFrom, request.DateTo);
		var limit = request.Limit <= 0 ? 10 : request.Limit;
		var rows = await _Repository.GetTopClientsAsync(limit, from, to, cancellationToken);
		return new TopClientsDto(rows.Select(TopClientDto.From).ToList());
	}
}
