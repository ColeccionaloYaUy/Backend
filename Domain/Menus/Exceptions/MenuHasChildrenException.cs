using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Menus.Exceptions;

public sealed class MenuHasChildrenException : DomainException {
	public MenuHasChildrenException(int id)
		: base(HttpStatusCode.Conflict,
			   "MenuHasChildren",
			   $"Menu item #{id} cannot be deleted because other items reference it as parent.") { }
}
