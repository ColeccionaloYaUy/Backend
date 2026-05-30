using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderListItemDto>> {
	private readonly IOrderRepository _Repository;

	public GetOrdersQueryHandler(IOrderRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<OrderListItemDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken) {
		var status = OrderStatusParser.ToDbString(request.Status);
		var data = await _Repository.GetPagedAsync(request.Page, request.PageSize, status, request.DateFrom, request.DateTo, request.ClientId, cancellationToken);
		return data.ToPagedResult(request.PageSize, OrderListItemDto.From);
	}
}
