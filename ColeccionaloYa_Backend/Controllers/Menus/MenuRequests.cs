using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Menus;

public class CreateMenuRequest {
	[Required]
	[MaxLength(20)]
	public string Name { get; set; } = string.Empty;

	public int IdTag { get; set; }

	public int Order { get; set; }

	public int? IdMenuReferenced { get; set; }

	public bool IsCategoryFilter { get; set; }
}

public class UpdateMenuRequest {
	[MaxLength(20)]
	public string? Name { get; set; }

	public int? IdTag { get; set; }

	public int? Order { get; set; }

	public int? IdMenuReferenced { get; set; }

	public bool? IsCategoryFilter { get; set; }
}

public class ReorderMenuRequest {
	[Required]
	[MinLength(1)]
	public List<ReorderMenuItemRequest> Items { get; set; } = new();
}

public class ReorderMenuItemRequest {
	public int IdMenu { get; set; }
	public int Order { get; set; }
}
