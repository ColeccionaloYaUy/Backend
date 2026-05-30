using ColeccionaloYa.Domain.Menus.Exceptions;
using ColeccionaloYa.Persistence.Menus.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Menus.GetMenuItemById;

public class GetMenuItemByIdQueryHandler : IRequestHandler<GetMenuItemByIdQuery, MenuItemDto> {
	private readonly IMenuRepository _Repository;

	public GetMenuItemByIdQueryHandler(IMenuRepository repository) {
		_Repository = repository;
	}

	public async Task<MenuItemDto> Handle(GetMenuItemByIdQuery request, CancellationToken cancellationToken) {
		var item = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new MenuItemNotFoundException(request.Id);
		return MenuItemDto.From(item);
	}
}
