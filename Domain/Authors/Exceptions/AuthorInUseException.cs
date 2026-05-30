using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Authors.Exceptions;

public sealed class AuthorInUseException : DomainException {
	public AuthorInUseException(int id)
		: base(HttpStatusCode.Conflict,
			   "AuthorInUse",
			   $"Author #{id} cannot be deleted because it is linked to one or more books.") { }
}
