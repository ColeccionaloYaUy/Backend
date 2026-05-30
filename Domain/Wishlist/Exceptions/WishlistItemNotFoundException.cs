using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Wishlist.Exceptions;

public sealed class WishlistItemNotFoundException : DomainException {
	public WishlistItemNotFoundException(int productId)
		: base(HttpStatusCode.NotFound,
			   "WishlistItemNotFound",
			   $"Product #{productId} is not in the wishlist.") { }
}
