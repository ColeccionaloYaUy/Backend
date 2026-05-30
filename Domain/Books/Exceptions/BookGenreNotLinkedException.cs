using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Books.Exceptions;

public sealed class BookGenreNotLinkedException : DomainException {
	public BookGenreNotLinkedException(int bookId, int genreId)
		: base(HttpStatusCode.NotFound,
			   "BookGenreNotLinked",
			   $"Genre #{genreId} is not linked to book #{bookId}.") { }
}
