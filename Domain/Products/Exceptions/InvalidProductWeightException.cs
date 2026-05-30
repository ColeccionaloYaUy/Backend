using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Products.Exceptions;

public sealed class InvalidProductWeightException : DomainException {
	public InvalidProductWeightException(decimal weight)
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidProductWeight",
			   $"Product weight '{weight}' is invalid; it must be zero or greater.") { }
}
