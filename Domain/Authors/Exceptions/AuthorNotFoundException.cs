using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Authors.Exceptions;

public sealed class AuthorNotFoundException : DomainException {
	public AuthorNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "AuthorNotFound",
			   $"Author #{id} not found.") { }
}
