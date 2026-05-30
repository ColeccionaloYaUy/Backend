using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Books.Exceptions;

public sealed class BookAuthorNotLinkedException : DomainException {
	public BookAuthorNotLinkedException(int bookId, int authorId)
		: base(HttpStatusCode.NotFound,
			   "BookAuthorNotLinked",
			   $"Author #{authorId} is not linked to book #{bookId}.") { }
}
