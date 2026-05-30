using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Genres.Exceptions;

public sealed class InvalidGenreReferenceException : DomainException {
	public InvalidGenreReferenceException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidGenreReference",
			   "One or more of the referenced genres do not exist.") { }
}
