namespace ColeccionaloYa.Persistence.Products.ReadModels;

public record ProductCatalogItem(
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
	ActiveDiscountInfo? Discount,
	string? TypeBook
);
