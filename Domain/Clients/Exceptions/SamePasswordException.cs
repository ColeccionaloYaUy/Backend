using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Clients.Exceptions;

public sealed class SamePasswordException : DomainException {
	public SamePasswordException()
		: base(HttpStatusCode.Conflict,
			   "SamePassword",
			   "The new password must be different from the current password.") { }
}
