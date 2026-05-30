using ColeccionaloYa.Domain.Menus.Exceptions;
using ColeccionaloYa.Persistence.Menus.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Menus.UpdateMenuItem;

public class UpdateMenuItemCommandHandler : IRequestHandler<UpdateMenuItemCommand, MenuItemDto> {
	private readonly IMenuRepository _Repository;

	public UpdateMenuItemCommandHandler(IMenuRepository repository) {
		_Repository = repository;
	}

	public async Task<MenuItemDto> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken) {
		var item = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new MenuItemNotFoundException(request.Id);

		if (request.IdTag.HasValue && !await _Repository.TagExistsAsync(request.IdTag.Value, cancellationToken)) {
			throw new InvalidMenuReferenceException();
		}

		if (request.IdMenuReferenced.HasValue) {
			if (!await _Repository.ExistsAsync(request.IdMenuReferenced.Value, cancellationToken)) {
				throw new InvalidMenuReferenceException();
			}

			if (await _Repository.WouldCreateCycleAsync(request.Id, request.IdMenuReferenced.Value, cancellationToken)) {
				throw new MenuCircularReferenceException();
			}
		}

		item.Update(request.Name, request.IdTag, request.Order, request.IdMenuReferenced, request.IsCategoryFilter);
		await _Repository.UpdateAsync(item, cancellationToken);
		return MenuItemDto.From(item);
	}
}
