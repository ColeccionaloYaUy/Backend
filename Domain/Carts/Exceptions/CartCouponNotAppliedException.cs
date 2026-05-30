using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Carts.Exceptions;

public sealed class CartCouponNotAppliedException : DomainException {
	public CartCouponNotAppliedException()
		: base(HttpStatusCode.NotFound,
			   "CartCouponNotApplied",
			   "The cart does not have a coupon applied.") { }
}
