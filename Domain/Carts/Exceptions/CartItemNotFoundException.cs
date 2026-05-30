using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Carts.Exceptions;

public sealed class CartItemNotFoundException : DomainException {
	public CartItemNotFoundException(int productId)
		: base(HttpStatusCode.NotFound,
			   "CartItemNotFound",
			   $"Product #{productId} is not in the cart.") { }
}
