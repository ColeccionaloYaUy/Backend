using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Books.Exceptions;

public sealed class BookNotFoundException : DomainException {
	public BookNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "BookNotFound",
			   $"Book #{id} not found.") { }
}
