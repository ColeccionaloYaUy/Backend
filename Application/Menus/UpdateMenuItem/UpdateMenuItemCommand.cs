using MediatR;

namespace ColeccionaloYa.Application.Menus.UpdateMenuItem;

public record UpdateMenuItemCommand(
	int Id,
	string? Name,
	int? IdTag,
	int? Order,
	int? IdMenuReferenced,
	bool? IsCategoryFilter
) : IRequest<MenuItemDto>;
