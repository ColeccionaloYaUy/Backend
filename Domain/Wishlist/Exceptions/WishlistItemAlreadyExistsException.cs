using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Wishlist.Exceptions;

public sealed class WishlistItemAlreadyExistsException : DomainException {
	public WishlistItemAlreadyExistsException(int productId)
		: base(HttpStatusCode.Conflict,
			   "WishlistItemAlreadyExists",
			   $"Product #{productId} is already in the wishlist.") { }
}
