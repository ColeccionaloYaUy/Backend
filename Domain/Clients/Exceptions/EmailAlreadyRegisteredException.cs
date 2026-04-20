using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Clients.Exceptions;

public sealed class EmailAlreadyRegisteredException : DomainException {
	public EmailAlreadyRegisteredException()
		: base(HttpStatusCode.Conflict,
			   "EmailAlreadyRegistered",
			   "The email address is already registered.") { }
}
