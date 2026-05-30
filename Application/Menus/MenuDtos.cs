using ColeccionaloYa.Domain.Menus;

namespace ColeccionaloYa.Application.Menus;

public record MenuItemDto(
	int IdMenu,
	string Name,
	int IdTag,
	int Order,
	int? IdMenuReferenced,
	bool IsCategoryFilter
) {
	public static MenuItemDto From(MenuItem item) =>
		new(item.Id, item.Name, item.TagId, item.Order, item.MenuReferencedId, item.IsCategoryFilter);
}

public record MenuTreeDto(
	int IdMenu,
	string Name,
	int IdTag,
	int Order,
	int? IdMenuReferenced,
	bool IsCategoryFilter,
	List<MenuTreeDto> Children
) {
	public static List<MenuTreeDto> BuildTree(IReadOnlyCollection<MenuItem> items) {
		var childrenByParent = items
			.GroupBy(i => i.MenuReferencedId)
			.ToDictionary(g => g.Key, g => g.OrderBy(i => i.Order).ThenBy(i => i.Id).ToList());

		List<MenuTreeDto> BuildLevel(int? parentId) {
			if (!childrenByParent.TryGetValue(parentId, out var nodes)) {
				return new List<MenuTreeDto>();
			}

			return nodes.Select(n => new MenuTreeDto(
				n.Id, n.Name, n.TagId, n.Order, n.MenuReferencedId, n.IsCategoryFilter,
				BuildLevel(n.Id))).ToList();
		}

		return BuildLevel(null);
	}
}
