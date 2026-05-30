using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Packs;

public class CreatePackItemRequest {
	public int IdProduct { get; set; }

	[Range(1, int.MaxValue)]
	public int Quantity { get; set; }
}

public class CreatePackRequest {
	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[MaxLength(100)]
	public string ShortDescription { get; set; } = string.Empty;

	[Required]
	public string LongDescription { get; set; } = string.Empty;

	[Range(0, double.MaxValue)]
	public decimal Price { get; set; }

	[Range(0, double.MaxValue)]
	public decimal Weight { get; set; }

	public List<CreatePackItemRequest> Items { get; set; } = new();
}

public class UpdatePackRequest {
	[MaxLength(100)]
	public string? Name { get; set; }

	[MaxLength(100)]
	public string? ShortDescription { get; set; }

	public string? LongDescription { get; set; }

	[Range(0, double.MaxValue)]
	public decimal? Price { get; set; }

	[Range(0, double.MaxValue)]
	public decimal? Weight { get; set; }
}

public class AddPackProductRequest {
	public int IdProduct { get; set; }

	[Range(1, int.MaxValue)]
	public int Quantity { get; set; }
}

public class UpdatePackProductRequest {
	[Range(1, int.MaxValue)]
	public int Quantity { get; set; }
}
