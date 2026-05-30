using MediatR;

namespace ColeccionaloYa.Application.Menus.CreateMenuItem;

public record CreateMenuItemCommand(
	string Name,
	int IdTag,
	int Order,
	int? IdMenuReferenced,
	bool IsCategoryFilter
) : IRequest<MenuItemDto>;
