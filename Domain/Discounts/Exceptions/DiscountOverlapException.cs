using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Discounts.Exceptions;

public sealed class DiscountOverlapException : DomainException {
	public DiscountOverlapException(int productId)
		: base(HttpStatusCode.Conflict,
			   "DiscountOverlap",
			   $"Product #{productId} already has an active discount overlapping the given date range.") { }
}
