namespace ColeccionaloYa.Persistence.Carts.ReadModels;

public record CartItemData(
	int IdProduct,
	string Name,
	string? CoverUrl,
	decimal Price,
	decimal DiscountedPrice,
	int Quantity,
	DateTime AddedDate,
	int CurrentStock
);

public record CartCouponData(int IdCoupon, string Name, string Token, decimal Porcentage);

public record CartData(
	int IdCart,
	int IdClient,
	int? IdCoupon,
	DateTime CreationDate,
	DateTime UpdatedDate,
	List<CartItemData> Items,
	CartCouponData? Coupon
);
