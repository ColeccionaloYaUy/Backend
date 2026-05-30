using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetOrdersByStatus;

public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, OrdersByStatusDto> {
	private readonly IDashboardRepository _Repository;

	public GetOrdersByStatusQueryHandler(IDashboardRepository repository) {
		_Repository = repository;
	}

	public async Task<OrdersByStatusDto> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken) {
		var (from, to) = DashboardDateRange.Resolve(request.DateFrom, request.DateTo);
		var rows = await _Repository.GetOrdersByStatusAsync(from, to, cancellationToken);
		var totalOrders = rows.Sum(r => r.Count);

		var data = rows.Select(r => new OrdersByStatusItemDto(
			r.Status, r.Count, r.TotalValue,
			totalOrders == 0 ? 0 : Math.Round((decimal)r.Count / totalOrders * 100, 2))).ToList();

		return new OrdersByStatusDto(data, totalOrders);
	}
}
