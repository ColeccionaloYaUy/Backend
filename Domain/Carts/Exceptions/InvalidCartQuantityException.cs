using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Carts.Exceptions;

public sealed class InvalidCartQuantityException : DomainException {
	public InvalidCartQuantityException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidCartQuantity",
			   "Cart item quantity must be greater than zero.") { }
}
