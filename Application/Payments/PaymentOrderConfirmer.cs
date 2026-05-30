using ColeccionaloYa.Domain.Orders;
using ColeccionaloYa.Persistence.Orders.Interfaces;

namespace ColeccionaloYa.Application.Payments;

public static class PaymentOrderConfirmer {
	/// <summary>
	/// When a payment is approved, advances the order from 'pending' to 'confirmed' (idempotent for other states).
	/// Must be invoked within an active transaction by the caller.
	/// </summary>
	public static async Task ConfirmIfPendingAsync(IOrderRepository orderRepository, int orderId, CancellationToken cancellationToken) {
		var order = await orderRepository.GetEntityAsync(orderId, cancellationToken);
		if (order is null || order.Status != OrderStatus.Pending) {
			return;
		}

		order.ChangeStatus(OrderStatus.Confirmed);
		await orderRepository.UpdateStatusAsync(order, cancellationToken);
		await orderRepository.AddStatusHistoryAsync(order.Id, OrderStatus.Confirmed, "Payment approved", null, cancellationToken);
	}
}
