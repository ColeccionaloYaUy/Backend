using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Roles.Exceptions;

public sealed class DuplicateRoleNameException : DomainException {
	public DuplicateRoleNameException(string name)
		: base(HttpStatusCode.Conflict,
			   "DuplicateRoleName",
			   $"A role with the name '{name}' already exists.") { }
}
