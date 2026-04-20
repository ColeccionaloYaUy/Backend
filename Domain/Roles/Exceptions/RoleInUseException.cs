using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Roles.Exceptions;

public sealed class RoleInUseException : DomainException {
	public RoleInUseException(int id)
		: base(HttpStatusCode.Conflict,
			   "RoleInUse",
			   $"Role #{id} cannot be deleted because it is assigned to one or more clients.") { }
}
