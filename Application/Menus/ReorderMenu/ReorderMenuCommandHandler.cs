using ColeccionaloYa.Domain.Menus.Exceptions;
using ColeccionaloYa.Persistence.Menus.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Menus.ReorderMenu;

public class ReorderMenuCommandHandler : IRequestHandler<ReorderMenuCommand, List<MenuTreeDto>> {
	private readonly IMenuRepository _Repository;

	public ReorderMenuCommandHandler(IMenuRepository repository) {
		_Repository = repository;
	}

	public async Task<List<MenuTreeDto>> Handle(ReorderMenuCommand request, CancellationToken cancellationToken) {
		foreach (var item in request.Items) {
			if (!await _Repository.ExistsAsync(item.IdMenu, cancellationToken)) {
				throw new InvalidMenuReferenceException();
			}
		}

		await _Repository.ReorderAsync(
			request.Items.Select(i => (i.IdMenu, i.Order)).ToList(),
			cancellationToken);

		var items = await _Repository.GetAllAsync(cancellationToken);
		return MenuTreeDto.BuildTree(items);
	}
}
