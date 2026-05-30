using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Tags.Exceptions;

public sealed class InvalidTagReferenceException : DomainException {
	public InvalidTagReferenceException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidTagReference",
			   "One or more of the referenced tags do not exist.") { }
}
