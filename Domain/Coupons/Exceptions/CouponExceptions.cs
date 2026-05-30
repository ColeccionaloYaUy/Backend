using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Coupons.Exceptions;

public sealed class CouponNotFoundException : DomainException {
	public CouponNotFoundException(int id)
		: base(HttpStatusCode.NotFound, "CouponNotFound", $"Coupon #{id} not found.") { }
}

public sealed class CouponTokenNotFoundException : DomainException {
	public CouponTokenNotFoundException(string token)
		: base(HttpStatusCode.NotFound, "CouponTokenNotFound", $"Coupon with token '{token}' not found.") { }
}

public sealed class DuplicateCouponTokenException : DomainException {
	public DuplicateCouponTokenException(string token)
		: base(HttpStatusCode.Conflict, "DuplicateCouponToken", $"A coupon with token '{token}' already exists.") { }
}

public sealed class InvalidCouponPercentageException : DomainException {
	public InvalidCouponPercentageException(decimal porcentage)
		: base(HttpStatusCode.UnprocessableEntity, "InvalidCouponPercentage",
			   $"Coupon percentage '{porcentage}' is invalid; it must be between 0 and 100.") { }
}

public sealed class CouponAlreadyActiveException : DomainException {
	public CouponAlreadyActiveException(int id)
		: base(HttpStatusCode.Conflict, "CouponAlreadyActive", $"Coupon #{id} is already active.") { }
}

public sealed class CouponAlreadyInactiveException : DomainException {
	public CouponAlreadyInactiveException(int id)
		: base(HttpStatusCode.Conflict, "CouponAlreadyInactive", $"Coupon #{id} is already inactive.") { }
}

public sealed class CouponExpiredException : DomainException {
	public CouponExpiredException(int id)
		: base(HttpStatusCode.Conflict, "CouponExpired", $"Coupon #{id} has expired.") { }
}

public sealed class CouponNotUsableException : DomainException {
	public CouponNotUsableException()
		: base(HttpStatusCode.Conflict, "CouponNotUsable", "The coupon is inactive, expired or not assigned to the client.") { }
}

public sealed class CouponClientAlreadyAssignedException : DomainException {
	public CouponClientAlreadyAssignedException()
		: base(HttpStatusCode.Conflict, "CouponClientAlreadyAssigned", "One or more clients already have the coupon assigned.") { }
}

public sealed class CouponClientNotAssignedException : DomainException {
	public CouponClientNotAssignedException(int couponId, int clientId)
		: base(HttpStatusCode.NotFound, "CouponClientNotAssigned", $"Coupon #{couponId} is not assigned to client #{clientId}.") { }
}
