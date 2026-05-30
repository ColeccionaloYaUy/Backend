using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Clients.Exceptions;

public sealed class InvalidClientReferenceException : DomainException {
	public InvalidClientReferenceException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidClientReference",
			   "One or more of the referenced clients do not exist.") { }
}
