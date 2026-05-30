using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetSummary;

public class GetSummaryQueryHandler : IRequestHandler<GetSummaryQuery, DashboardSummaryDto> {
	private readonly IDashboardRepository _Repository;

	public GetSummaryQueryHandler(IDashboardRepository repository) {
		_Repository = repository;
	}

	public async Task<DashboardSummaryDto> Handle(GetSummaryQuery request, CancellationToken cancellationToken) {
		var (from, to) = DashboardDateRange.Resolve(request.DateFrom, request.DateTo);
		var totals = await _Repository.GetSummaryAsync(from, to, cancellationToken);
		var avgTicket = totals.OrdersCount == 0 ? 0 : Math.Round(totals.TotalSales / totals.OrdersCount, 2);
		return new DashboardSummaryDto(totals.TotalSales, totals.OrdersCount, totals.NewClients, avgTicket,
			totals.TotalUnitsSold, totals.ActiveClients, from, to);
	}
}
