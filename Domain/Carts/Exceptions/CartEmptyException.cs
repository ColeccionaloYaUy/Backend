using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Carts.Exceptions;

public sealed class CartEmptyException : DomainException {
	public CartEmptyException()
		: base(HttpStatusCode.Conflict,
			   "CartEmpty",
			   "The cart is empty.") { }
}
