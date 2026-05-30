using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Products;

public class CreateProductRequest {
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

	[Required]
	public string Type { get; set; } = string.Empty;

	[Range(0, double.MaxValue)]
	public decimal Weight { get; set; }

	public bool IsFeatured { get; set; }
}
