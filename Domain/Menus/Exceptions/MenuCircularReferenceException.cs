using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Menus.Exceptions;

public sealed class MenuCircularReferenceException : DomainException {
	public MenuCircularReferenceException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "MenuCircularReference",
			   "The requested parent reference would create a circular menu structure.") { }
}
