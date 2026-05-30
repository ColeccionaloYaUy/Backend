using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.GetMyOrders;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderListItemDto>> {
	private readonly IOrderRepository _Repository;

	public GetMyOrdersQueryHandler(IOrderRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<OrderListItemDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken) {
		var status = OrderStatusParser.ToDbString(request.Status);
		var data = await _Repository.GetPagedAsync(request.Page, request.PageSize, status, null, null, request.ClientId, cancellationToken);
		return data.ToPagedResult(request.PageSize, OrderListItemDto.From);
	}
}
