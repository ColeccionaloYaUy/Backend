using MediatR;

namespace ColeccionaloYa.Application.Menus.GetMenu;

public record GetMenuQuery() : IRequest<List<MenuTreeDto>>;
