using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetRealtimeSummary;

public class GetRealtimeSummaryQueryHandler : IRequestHandler<GetRealtimeSummaryQuery, RealtimeSummaryDto> {
	private const int StockThreshold = 5;

	private readonly IDashboardRepository _Repository;

	public GetRealtimeSummaryQueryHandler(IDashboardRepository repository) {
		_Repository = repository;
	}

	public async Task<RealtimeSummaryDto> Handle(GetRealtimeSummaryQuery request, CancellationToken cancellationToken) {
		var row = await _Repository.GetRealtimeSummaryAsync(StockThreshold, cancellationToken);
		var lastOrder = await _Repository.GetLastOrderAsync(cancellationToken);

		return new RealtimeSummaryDto(
			row.OrdersLast24h, row.RevenueLast24h, row.NewClientsLast24h, row.ActiveCartsNow,
			row.PendingOrders, row.ProcessingOrders, row.ShippedOrders, row.PendingPayments, row.StockAlerts,
			lastOrder is null ? null : new RealtimeLastOrderDto(lastOrder.IdOrder, lastOrder.ClientName, lastOrder.Total, lastOrder.CreationDate));
	}
}
