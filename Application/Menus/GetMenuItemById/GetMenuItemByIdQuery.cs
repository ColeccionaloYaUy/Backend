using MediatR;

namespace ColeccionaloYa.Application.Menus.GetMenuItemById;

public record GetMenuItemByIdQuery(int Id) : IRequest<MenuItemDto>;
