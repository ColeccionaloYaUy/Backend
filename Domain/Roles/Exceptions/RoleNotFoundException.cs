using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Roles.Exceptions;

public sealed class RoleNotFoundException : DomainException {
	public RoleNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "RoleNotFound",
			   $"Role #{id} not found.") { }
}
