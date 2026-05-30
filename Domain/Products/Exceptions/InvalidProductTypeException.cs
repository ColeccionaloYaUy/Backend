using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Products.Exceptions;

public sealed class InvalidProductTypeException : DomainException {
	public InvalidProductTypeException(string type)
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidProductType",
			   $"Product type '{type}' is invalid; allowed values are 'book', 'pack' or 'object'.") { }
}
