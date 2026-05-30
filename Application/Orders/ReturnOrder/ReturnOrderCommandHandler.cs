using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Orders;
using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.ReturnOrder;

public class ReturnOrderCommandHandler : IRequestHandler<ReturnOrderCommand, OrderDetailDto> {
	private readonly ICConnection _Connection;
	private readonly IOrderRepository _Repository;

	public ReturnOrderCommandHandler(ICConnection connection, IOrderRepository repository) {
		_Connection = connection;
		_Repository = repository;
	}

	public async Task<OrderDetailDto> Handle(ReturnOrderCommand request, CancellationToken cancellationToken) {
		await OrderAccessGuard.EnsureAccessAsync(_Repository, request.OrderId, request.RequesterId, request.IsAdmin, cancellationToken);

		var order = await _Repository.GetEntityAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);

		order.MarkReturned(request.Reason);

		await _Connection.BeginTransaction(cancellationToken);
		try {
			await _Repository.UpdateStatusAsync(order, cancellationToken);
			await _Repository.AddStatusHistoryAsync(order.Id, OrderStatus.Returned, request.Reason, request.RequesterId, cancellationToken);
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
