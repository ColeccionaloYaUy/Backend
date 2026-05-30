using ColeccionaloYa.Domain.Menus;
using ColeccionaloYa.Domain.Menus.Exceptions;
using ColeccionaloYa.Persistence.Menus.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Menus.CreateMenuItem;

public class CreateMenuItemCommandHandler : IRequestHandler<CreateMenuItemCommand, MenuItemDto> {
	private readonly IMenuRepository _Repository;

	public CreateMenuItemCommandHandler(IMenuRepository repository) {
		_Repository = repository;
	}

	public async Task<MenuItemDto> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken) {
		if (!await _Repository.TagExistsAsync(request.IdTag, cancellationToken)) {
			throw new InvalidMenuReferenceException();
		}

		if (request.IdMenuReferenced.HasValue
			&& !await _Repository.ExistsAsync(request.IdMenuReferenced.Value, cancellationToken)) {
			throw new InvalidMenuReferenceException();
		}

		var item = MenuItem.Create(request.Name, request.IdTag, request.Order, request.IdMenuReferenced, request.IsCategoryFilter);
		await _Repository.CreateAsync(item, cancellationToken);
		return MenuItemDto.From(item);
	}
}
