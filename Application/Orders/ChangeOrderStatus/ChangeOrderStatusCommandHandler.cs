using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.ChangeOrderStatus;

public class ChangeOrderStatusCommandHandler : IRequestHandler<ChangeOrderStatusCommand, OrderDetailDto> {
	private readonly ICConnection _Connection;
	private readonly IOrderRepository _Repository;

	public ChangeOrderStatusCommandHandler(ICConnection connection, IOrderRepository repository) {
		_Connection = connection;
		_Repository = repository;
	}

	public async Task<OrderDetailDto> Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken) {
		var target = OrderStatusParser.Parse(request.Status);

		var order = await _Repository.GetEntityAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);

		order.ChangeStatus(target);

		await _Connection.BeginTransaction(cancellationToken);
		try {
			await _Repository.UpdateStatusAsync(order, cancellationToken);
			await _Repository.AddStatusHistoryAsync(order.Id, target, request.Reason, request.ChangedBy, cancellationToken);
			await _Connection.CommitTransaction(cancellationToken);
		} catch {
			await _Connection.CancelTransaction(cancellationToken);
			throw;
		}

		var detail = await _Repository.GetDetailAsync(order.Id, cancellationToken)
			?? throw new OrderNotFoundException(order.Id);
		return OrderDetailDto.From(detail);
	}
}
