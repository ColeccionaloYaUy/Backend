using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Products.Exceptions;

public sealed class InvalidProductPriceException : DomainException {
	public InvalidProductPriceException(decimal price)
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidProductPrice",
			   $"Product price '{price}' is invalid; it must be zero or greater.") { }
}
