using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Products.Exceptions;

public sealed class ProductTagAlreadyLinkedException : DomainException {
	public ProductTagAlreadyLinkedException()
		: base(HttpStatusCode.Conflict,
			   "ProductTagAlreadyLinked",
			   "One or more tags are already assigned to the product.") { }
}
