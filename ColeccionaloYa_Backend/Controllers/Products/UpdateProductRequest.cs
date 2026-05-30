using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Products;

public class UpdateProductRequest {
	[MaxLength(100)]
	public string? Name { get; set; }

	[MaxLength(100)]
	public string? ShortDescription { get; set; }

	public string? LongDescription { get; set; }

	[Range(0, double.MaxValue)]
	public decimal? Price { get; set; }

	public string? Type { get; set; }

	[Range(0, double.MaxValue)]
	public decimal? Weight { get; set; }

	public bool? IsFeatured { get; set; }
}
