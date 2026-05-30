using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Discounts.Exceptions;

public sealed class InvalidDiscountActionException : DomainException {
	public InvalidDiscountActionException(string action)
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidDiscountAction",
			   $"Discount action '{action}' is invalid; allowed values are 'activate' or 'deactivate'.") { }
}
