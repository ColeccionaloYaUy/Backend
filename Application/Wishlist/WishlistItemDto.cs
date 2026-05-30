using ColeccionaloYa.Persistence.Wishlist.ReadModels;

namespace ColeccionaloYa.Application.Wishlist;

public record WishlistDiscountDto(int IdDiscount, decimal Porcentage, decimal FinalPrice);

public record WishlistItemDto(
	int IdProduct,
	string Name,
	string ShortDescription,
	decimal Price,
	string Type,
	string? CoverUrl,
	int CurrentStock,
	WishlistDiscountDto? Discount
) {
	public static WishlistItemDto From(WishlistProductItem item) =>
		new(item.IdProduct, item.Name, item.ShortDescription, item.Price, item.Type,
			item.CoverUrl, item.CurrentStock,
			item.Discount is null
				? null
				: new WishlistDiscountDto(item.Discount.IdDiscount, item.Discount.Porcentage, item.Discount.FinalPrice));
}

public record WishlistAddedDto(int IdClient, int IdProduct);

public record WishlistCheckDto(bool InWishlist);
