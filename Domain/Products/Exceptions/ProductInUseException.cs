using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Products.Exceptions;

public sealed class ProductInUseException : DomainException {
	public ProductInUseException(int id)
		: base(HttpStatusCode.Conflict,
			   "ProductInUse",
			   $"Product #{id} cannot be deleted because it is part of active orders or composes active packs.") { }
}
