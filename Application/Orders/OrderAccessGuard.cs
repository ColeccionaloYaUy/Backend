using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;

namespace ColeccionaloYa.Application.Orders;

public static class OrderAccessGuard {
	/// <summary>
	/// Ensures the order exists and, for non-admin callers, that it belongs to the requester.
	/// </summary>
	public static async Task EnsureAccessAsync(
		IOrderRepository repository, int orderId, int requesterId, bool isAdmin, CancellationToken cancellationToken) {
		var ownerId = await repository.GetOwnerClientIdAsync(orderId, cancellationToken)
			?? throw new OrderNotFoundException(orderId);

		if (!isAdmin && ownerId != requesterId) {
			throw new OrderAccessDeniedException();
		}
	}
}
