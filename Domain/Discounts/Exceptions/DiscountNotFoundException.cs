using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Discounts.Exceptions;

public sealed class DiscountNotFoundException : DomainException {
	public DiscountNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "DiscountNotFound",
			   $"Discount #{id} not found.") { }
}
