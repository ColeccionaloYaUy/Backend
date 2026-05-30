using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.UpdateOrderTracking;

public class UpdateOrderTrackingCommandHandler : IRequestHandler<UpdateOrderTrackingCommand, OrderDetailDto> {
	private readonly IOrderRepository _Repository;

	public UpdateOrderTrackingCommandHandler(IOrderRepository repository) {
		_Repository = repository;
	}

	public async Task<OrderDetailDto> Handle(UpdateOrderTrackingCommand request, CancellationToken cancellationToken) {
		var order = await _Repository.GetEntityAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);

		order.SetTracking(request.Tracking);
		await _Repository.UpdateTrackingAsync(order.Id, order.Tracking, cancellationToken);

		var detail = await _Repository.GetDetailAsync(order.Id, cancellationToken)
			?? throw new OrderNotFoundException(order.Id);
		return OrderDetailDto.From(detail);
	}
}
