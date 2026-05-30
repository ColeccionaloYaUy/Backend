using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Orders.Exceptions;

public sealed class OrderAccessDeniedException : DomainException {
	public OrderAccessDeniedException()
		: base(HttpStatusCode.Forbidden, "OrderAccessDenied", "The order does not belong to the authenticated user.") { }
}

public sealed class EmptyOrderException : DomainException {
	public EmptyOrderException()
		: base(HttpStatusCode.Conflict, "EmptyOrder", "An order cannot be created without items.") { }
}

public sealed class OrderInvalidTransitionException : DomainException {
	public OrderInvalidTransitionException(int orderId, OrderStatus from, OrderStatus to)
		: base(HttpStatusCode.Conflict, "OrderInvalidTransition",
			   $"Order #{orderId} cannot transition from '{from.ToString().ToLowerInvariant()}' to '{to.ToString().ToLowerInvariant()}'.") { }
}

public sealed class OrderCannotBeCancelledException : DomainException {
	public OrderCannotBeCancelledException(int orderId)
		: base(HttpStatusCode.Conflict, "OrderCannotBeCancelled",
			   $"Order #{orderId} cannot be cancelled in its current status.") { }
}

public sealed class OrderCannotBeReturnedException : DomainException {
	public OrderCannotBeReturnedException(int orderId)
		: base(HttpStatusCode.Conflict, "OrderCannotBeReturned",
			   $"Order #{orderId} cannot be returned because it is not in 'delivered' status.") { }
}

public sealed class InvalidOrderStatusException : DomainException {
	public InvalidOrderStatusException(string status)
		: base(HttpStatusCode.UnprocessableEntity, "InvalidOrderStatus", $"Order status '{status}' is invalid.") { }
}

public sealed class InvalidOrderAddressException : DomainException {
	public InvalidOrderAddressException()
		: base(HttpStatusCode.UnprocessableEntity, "InvalidOrderAddress",
			   "The delivery or billing address does not belong to the client.") { }
}

public sealed class OrderInvoiceNotAvailableException : DomainException {
	public OrderInvoiceNotAvailableException(int orderId)
		: base(HttpStatusCode.Conflict, "OrderInvoiceNotAvailable",
			   $"Order #{orderId} is pending or cancelled and does not support an invoice.") { }
}
