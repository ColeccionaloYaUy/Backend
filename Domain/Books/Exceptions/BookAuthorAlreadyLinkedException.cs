using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Books.Exceptions;

public sealed class BookAuthorAlreadyLinkedException : DomainException {
	public BookAuthorAlreadyLinkedException()
		: base(HttpStatusCode.Conflict,
			   "BookAuthorAlreadyLinked",
			   "One or more authors are already linked to the book.") { }
}
