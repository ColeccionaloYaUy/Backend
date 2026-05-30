namespace ColeccionaloYa.Domain.Menus;

public class MenuItem {
	public int Id { get; internal set; }
	public string Name { get; internal set; } = string.Empty;
	public int TagId { get; internal set; }
	public int Order { get; internal set; }
	public int? MenuReferencedId { get; internal set; }
	public bool IsCategoryFilter { get; internal set; }

	internal MenuItem() { }

	public static MenuItem Create(string name, int tagId, int order, int? menuReferencedId, bool isCategoryFilter) {
		return new MenuItem {
			Name = name.Trim(),
			TagId = tagId,
			Order = order,
			MenuReferencedId = menuReferencedId,
			IsCategoryFilter = isCategoryFilter,
		};
	}

	public void Update(string? name, int? tagId, int? order, int? menuReferencedId, bool? isCategoryFilter) {
		if (name is not null) Name = name.Trim();
		if (tagId.HasValue) TagId = tagId.Value;
		if (order.HasValue) Order = order.Value;
		MenuReferencedId = menuReferencedId;
		if (isCategoryFilter.HasValue) IsCategoryFilter = isCategoryFilter.Value;
	}

	public void AssignId(int id) {
		Id = id;
	}
}
