using ColeccionaloYa.Persistence.Menus.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Menus.GetMenu;

public class GetMenuQueryHandler : IRequestHandler<GetMenuQuery, List<MenuTreeDto>> {
	private readonly IMenuRepository _Repository;

	public GetMenuQueryHandler(IMenuRepository repository) {
		_Repository = repository;
	}

	public async Task<List<MenuTreeDto>> Handle(GetMenuQuery request, CancellationToken cancellationToken) {
		var items = await _Repository.GetAllAsync(cancellationToken);
		return MenuTreeDto.BuildTree(items);
	}
}
