using ColeccionaloYa.Domain.Products;

namespace ColeccionaloYa.Application.Products;

public record ProductDto(
	int IdProduct,
	string Name,
	string ShortDescription,
	string LongDescription,
	decimal Price,
	string Type,
	decimal Weight,
	bool IsFeatured,
	DateOnly CreationDate
) {
	public static ProductDto From(Product p) =>
		new(p.Id, p.Name, p.ShortDescription, p.LongDescription, p.Price,
			p.Type.ToString().ToLowerInvariant(), p.Weight, p.IsFeatured, p.CreationDate);
}
