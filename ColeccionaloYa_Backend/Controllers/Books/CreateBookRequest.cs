using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Books;

public class CreateBookRequest {
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

	[Required]
	public string TypeBook { get; set; } = string.Empty;

	public List<int> AuthorIds { get; set; } = new();

	public List<int> GenreIds { get; set; } = new();

	public bool IsFeatured { get; set; }
}
