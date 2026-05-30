using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Discounts.Exceptions;

public sealed class InvalidDiscountPercentageException : DomainException {
	public InvalidDiscountPercentageException(decimal porcentage)
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidDiscountPercentage",
			   $"Discount percentage '{porcentage}' is invalid; it must be between 0 and 100.") { }
}
