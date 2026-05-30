using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetStockAlerts;

public class GetStockAlertsQueryHandler : IRequestHandler<GetStockAlertsQuery, StockAlertsDto> {
	private readonly IDashboardRepository _Repository;

	public GetStockAlertsQueryHandler(IDashboardRepository repository) {
		_Repository = repository;
	}

	public async Task<StockAlertsDto> Handle(GetStockAlertsQuery request, CancellationToken cancellationToken) {
		var threshold = request.Threshold <= 0 ? 5 : request.Threshold;
		var rows = await _Repository.GetStockAlertsAsync(threshold, cancellationToken);

		var data = rows.Select(r => {
			var daysUntilStockout = r.AvgDailySales <= 0 ? (int?)null : (int)Math.Floor(r.CurrentStock / r.AvgDailySales);
			var severity = r.CurrentStock <= 0 ? "critical" : r.CurrentStock <= threshold / 2 ? "low" : "warning";
			return new StockAlertDto(r.IdProduct, r.Name, r.Type, r.CoverUrl, r.CurrentStock, threshold,
				Math.Round(r.AvgDailySales, 2), daysUntilStockout, severity);
		}).ToList();

		return new StockAlertsDto(data, data.Count);
	}
}
