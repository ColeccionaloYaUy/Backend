using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Products.Exceptions;

public sealed class ProductTagNotLinkedException : DomainException {
	public ProductTagNotLinkedException(int productId, int tagId)
		: base(HttpStatusCode.NotFound,
			   "ProductTagNotLinked",
			   $"Tag #{tagId} is not linked to product #{productId}.") { }
}
