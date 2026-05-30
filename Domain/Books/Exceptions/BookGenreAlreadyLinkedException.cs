using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Books.Exceptions;

public sealed class BookGenreAlreadyLinkedException : DomainException {
	public BookGenreAlreadyLinkedException()
		: base(HttpStatusCode.Conflict,
			   "BookGenreAlreadyLinked",
			   "One or more genres are already linked to the book.") { }
}
