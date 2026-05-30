using ColeccionaloYa.Persistence.Products.ReadModels;

namespace ColeccionaloYa.Persistence.Wishlist.ReadModels;

public record WishlistProductItem(
	int IdProduct,
	string Name,
	string ShortDescription,
	decimal Price,
	string Type,
	string? CoverUrl,
	int CurrentStock,
	ActiveDiscountInfo? Discount
);
