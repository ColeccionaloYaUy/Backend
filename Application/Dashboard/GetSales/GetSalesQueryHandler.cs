using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetSales;

public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, SalesSeriesDto> {
	private static readonly string[] AllowedPeriods = { "day", "week", "month" };

	private readonly IDashboardRepository _Repository;

	public GetSalesQueryHandler(IDashboardRepository repository) {
		_Repository = repository;
	}

	public async Task<SalesSeriesDto> Handle(GetSalesQuery request, CancellationToken cancellationToken) {
		var period = AllowedPeriods.Contains(request.Period) ? request.Period : "day";
		var (from, to) = DashboardDateRange.Resolve(request.DateFrom, request.DateTo);

		var buckets = await _Repository.GetSalesSeriesAsync(period, from, to, request.Status, cancellationToken);
		var series = buckets.Select(b => new SalesSeriesPointDto(
			b.Date, SalesLabel.For(period, b.Date), b.Total, b.Subtotal, b.Taxes, b.Orders, b.Units)).ToList();

		var totalSales = series.Sum(s => s.Total);
		var totalOrders = series.Sum(s => s.Orders);
		var totalUnits = series.Sum(s => s.Units);
		var avgPerBucket = series.Count == 0 ? 0 : Math.Round(totalSales / series.Count, 2);

		return new SalesSeriesDto(period, series, new SalesTotalsDto(totalSales, totalOrders, totalUnits, avgPerBucket));
	}
}
