using ColeccionaloYa.Application.Dashboard;
using ColeccionaloYa.Application.Dashboard.GetCatalogSummary;
using ColeccionaloYa.Application.Dashboard.GetOrdersByStatus;
using ColeccionaloYa.Application.Dashboard.GetRealtimeSummary;
using ColeccionaloYa.Application.Dashboard.GetSales;
using ColeccionaloYa.Application.Dashboard.GetStockAlerts;
using ColeccionaloYa.Application.Dashboard.GetSummary;
using ColeccionaloYa.Application.Dashboard.GetTopClients;
using ColeccionaloYa.Application.Dashboard.GetTopProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Dashboard;

[Route("api/dashboard")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase {
	private readonly IMediator _Mediator;

	public DashboardController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet("summary")]
	public Task<DashboardSummaryDto> Summary(
		CancellationToken cancellationToken,
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null
	) =>
		_Mediator.Send(new GetSummaryQuery(dateFrom, dateTo), cancellationToken);

	[HttpGet("sales")]
	public Task<SalesSeriesDto> Sales(
		CancellationToken cancellationToken,
		[FromQuery] string period = "day",
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null,
		[FromQuery] string status = "valid"
	) =>
		_Mediator.Send(new GetSalesQuery(period, dateFrom, dateTo, status), cancellationToken);

	[HttpGet("top-products")]
	public Task<TopProductsDto> TopProducts(
		CancellationToken cancellationToken,
		[FromQuery] int limit = 10,
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null,
		[FromQuery] string? type = null
	) =>
		_Mediator.Send(new GetTopProductsQuery(limit, dateFrom, dateTo, type), cancellationToken);

	[HttpGet("top-clients")]
	public Task<TopClientsDto> TopClients(
		CancellationToken cancellationToken,
		[FromQuery] int limit = 10,
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null
	) =>
		_Mediator.Send(new GetTopClientsQuery(limit, dateFrom, dateTo), cancellationToken);

	[HttpGet("orders-by-status")]
	public Task<OrdersByStatusDto> OrdersByStatus(
		CancellationToken cancellationToken,
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null
	) =>
		_Mediator.Send(new GetOrdersByStatusQuery(dateFrom, dateTo), cancellationToken);

	[HttpGet("stock-alerts")]
	public Task<StockAlertsDto> StockAlerts(CancellationToken cancellationToken, [FromQuery] int threshold = 5) =>
		_Mediator.Send(new GetStockAlertsQuery(threshold), cancellationToken);

	[HttpGet("catalog-summary")]
	public Task<CatalogSummaryDto> CatalogSummary(CancellationToken cancellationToken) =>
		_Mediator.Send(new GetCatalogSummaryQuery(), cancellationToken);

	[HttpGet("realtime-summary")]
	public Task<RealtimeSummaryDto> RealtimeSummary(CancellationToken cancellationToken) =>
		_Mediator.Send(new GetRealtimeSummaryQuery(), cancellationToken);
}
