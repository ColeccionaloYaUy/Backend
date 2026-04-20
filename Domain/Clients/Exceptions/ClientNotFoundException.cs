using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Clients.Exceptions;

public sealed class ClientNotFoundException : DomainException {
	public ClientNotFoundException()
		: base(HttpStatusCode.NotFound,
			   "ClientNotFound",
			   "The client was not found.") { }

	public ClientNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "ClientNotFound",
			   $"Client #{id} not found.") { }
}
