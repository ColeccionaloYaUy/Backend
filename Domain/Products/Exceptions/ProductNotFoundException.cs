using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Products.Exceptions;

public sealed class ProductNotFoundException : DomainException {
	public ProductNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "ProductNotFound",
			   $"Product #{id} not found.") { }
}
