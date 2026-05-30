using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Authors.Exceptions;

public sealed class InvalidAuthorReferenceException : DomainException {
	public InvalidAuthorReferenceException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidAuthorReference",
			   "One or more of the referenced authors do not exist.") { }
}
