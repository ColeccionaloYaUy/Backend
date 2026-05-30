using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Menus.Exceptions;

public sealed class MenuItemNotFoundException : DomainException {
	public MenuItemNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "MenuItemNotFound",
			   $"Menu item #{id} not found.") { }
}
