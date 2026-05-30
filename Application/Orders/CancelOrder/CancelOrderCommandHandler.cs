using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Orders;
using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Domain.Stock;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDetailDto> {
	private readonly ICConnection _Connection;
	private readonly IOrderRepository _Repository;
	private readonly IStockRepository _StockRepository;

	public CancelOrderCommandHandler(ICConnection connection, IOrderRepository repository, IStockRepository stockRepository) {
		_Connection = connection;
		_Repository = repository;
		_StockRepository = stockRepository;
	}

	public async Task<OrderDetailDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken) {
		await OrderAccessGuard.EnsureAccessAsync(_Repository, request.OrderId, request.RequesterId, request.IsAdmin, cancellationToken);

		var order = await _Repository.GetEntityAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);

		order.Cancel(request.Reason);

		var lines = await _Repository.GetLineQuantitiesAsync(order.Id, cancellationToken);

		await _Connection.BeginTransaction(cancellationToken);
		try {
			await _Repository.UpdateStatusAsync(order, cancellationToken);
			await _Repository.AddStatusHistoryAsync(order.Id, OrderStatus.Cancelled, request.Reason, request.RequesterId, cancellationToken);

			// Release the reserved stock.
			foreach (var (productId, quantity) in lines) {
				var movement = StockMovement.Create(
					productId, StockMovementType.Entry, quantity, DateTime.UtcNow, order.Id, $"Cancellation of order #{order.Id}");
				await _StockRepository.CreateAsync(movement, cancellationToken);
			}

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
