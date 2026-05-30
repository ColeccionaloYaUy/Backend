using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.GetOrderTracking;

public class GetOrderTrackingQueryHandler : IRequestHandler<GetOrderTrackingQuery, OrderTrackingDto> {
	private readonly IOrderRepository _Repository;

	public GetOrderTrackingQueryHandler(IOrderRepository repository) {
		_Repository = repository;
	}

	public async Task<OrderTrackingDto> Handle(GetOrderTrackingQuery request, CancellationToken cancellationToken) {
		await OrderAccessGuard.EnsureAccessAsync(_Repository, request.OrderId, request.RequesterId, request.IsAdmin, cancellationToken);

		var data = await _Repository.GetTrackingAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);
		return OrderTrackingDto.From(data);
	}
}
