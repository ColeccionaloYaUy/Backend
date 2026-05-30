using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Books.Exceptions;

public sealed class InvalidBookTypeException : DomainException {
	public InvalidBookTypeException(string type)
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidBookType",
			   $"Book type '{type}' is invalid; allowed values are 'comic' or 'manga'.") { }
}
