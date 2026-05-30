using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Menus.Exceptions;

public sealed class InvalidMenuReferenceException : DomainException {
	public InvalidMenuReferenceException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidMenuReference",
			   "The referenced tag or parent menu item does not exist.") { }
}
