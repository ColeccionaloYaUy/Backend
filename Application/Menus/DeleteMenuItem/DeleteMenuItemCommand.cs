using MediatR;

namespace ColeccionaloYa.Application.Menus.DeleteMenuItem;

public record DeleteMenuItemCommand(int Id) : IRequest<Unit>;
