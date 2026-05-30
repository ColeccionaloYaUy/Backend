using ColeccionaloYa.Persistence.Products.ReadModels;

namespace ColeccionaloYa.Application.Products;

public record ProductSummaryDto(
	int IdProduct,
	string Name,
	string ShortDescription,
	decimal Price,
	string Type,
	decimal Weight,
	bool IsFeatured,
	DateOnly CreationDate,
	string? CoverUrl,
	int CurrentStock,
	DiscountInfoDto? Discount,
	string? TypeBook
) {
	public static ProductSummaryDto From(ProductCatalogItem item) =>
		new(item.IdProduct, item.Name, item.ShortDescription, item.Price, item.Type,
			item.Weight, item.IsFeatured, item.CreationDate, item.CoverUrl, item.CurrentStock,
			item.Discount is null ? null : DiscountInfoDto.From(item.Discount), item.TypeBook);
}
