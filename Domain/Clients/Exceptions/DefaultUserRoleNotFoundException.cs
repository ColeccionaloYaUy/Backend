using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Clients.Exceptions;

public sealed class DefaultUserRoleNotFoundException : DomainException {
	public DefaultUserRoleNotFoundException()
		: base(HttpStatusCode.Conflict,
			   "DefaultUserRoleNotFound",
			   "The default user role is not configured in the system.") { }
}
