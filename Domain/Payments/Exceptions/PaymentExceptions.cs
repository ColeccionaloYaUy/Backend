using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Payments.Exceptions;

public sealed class PaymentNotFoundException : DomainException {
	public PaymentNotFoundException(int id)
		: base(HttpStatusCode.NotFound, "PaymentNotFound", $"Payment #{id} not found.") { }
}

public sealed class PaymentAccessDeniedException : DomainException {
	public PaymentAccessDeniedException()
		: base(HttpStatusCode.Forbidden, "PaymentAccessDenied", "The payment does not belong to the authenticated user.") { }
}

public sealed class OrderAlreadyPaidException : DomainException {
	public OrderAlreadyPaidException(int orderId)
		: base(HttpStatusCode.Conflict, "OrderAlreadyPaid", $"Order #{orderId} already has an approved payment or is cancelled.") { }
}

public sealed class PaymentNotRefundableException : DomainException {
	public PaymentNotRefundableException(int id)
		: base(HttpStatusCode.Conflict, "PaymentNotRefundable", $"Payment #{id} is not in 'approved' status and cannot be refunded.") { }
}

public sealed class InvalidRefundAmountException : DomainException {
	public InvalidRefundAmountException(int id)
		: base(HttpStatusCode.UnprocessableEntity, "InvalidRefundAmount", $"The refund amount for payment #{id} exceeds the payment amount.") { }
}

public sealed class PaymentNotCancellableException : DomainException {
	public PaymentNotCancellableException(int id)
		: base(HttpStatusCode.Conflict, "PaymentNotCancellable", $"Payment #{id} is not in 'pending' or 'in_process' status.") { }
}

public sealed class PaymentNotRetryableException : DomainException {
	public PaymentNotRetryableException(int id)
		: base(HttpStatusCode.Conflict, "PaymentNotRetryable", $"Payment #{id} is not in 'rejected' or 'cancelled' status.") { }
}

public sealed class PaymentGatewayException : DomainException {
	public PaymentGatewayException(string detail)
		: base(HttpStatusCode.BadGateway, "PaymentGatewayError", $"Error communicating with the payment gateway: {detail}") { }
}
