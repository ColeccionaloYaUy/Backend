using MediatR;

namespace ColeccionaloYa.Application.Menus.ReorderMenu;

public record ReorderMenuItem(int IdMenu, int Order);

public record ReorderMenuCommand(IReadOnlyList<ReorderMenuItem> Items) : IRequest<List<MenuTreeDto>>;
