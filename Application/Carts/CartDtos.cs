using ColeccionaloYa.Domain.Carts;
using ColeccionaloYa.Persistence.Carts.ReadModels;

namespace ColeccionaloYa.Application.Carts;

public record CartItemDto(
	int IdProduct,
	string Name,
	string? CoverUrl,
	decimal Price,
	decimal DiscountedPrice,
	int Quantity,
	decimal Subtotal,
	DateTime AddedDate,
	int CurrentStock
);

public record CartCouponDto(int IdCoupon, string Name, string Token, decimal Porcentage);

public record CartDto(
	int IdCart,
	int IdClient,
	int? IdCoupon,
	DateTime CreationDate,
	DateTime UpdatedDate,
	List<CartItemDto> Items,
	decimal Subtotal,
	decimal Discounts,
	decimal Taxes,
	decimal Total,
	CartCouponDto? Coupon
) {
	public static CartDto From(CartData data) {
		var items = data.Items.Select(i => new CartItemDto(
			i.IdProduct, i.Name, i.CoverUrl, i.Price, i.DiscountedPrice, i.Quantity,
			Math.Round(i.DiscountedPrice * i.Quantity, 2), i.AddedDate, i.CurrentStock)).ToList();

		var subtotal = items.Sum(i => i.Subtotal);
		var couponDiscount = data.Coupon is null
			? 0m
			: Math.Round(subtotal * data.Coupon.Porcentage / 100m, 2);
		var discounts = couponDiscount;
		var netBeforeTax = subtotal - discounts;
		var taxes = Math.Round(netBeforeTax * CartTax.Rate, 2);
		var total = netBeforeTax + taxes;

		return new CartDto(
			data.IdCart, data.IdClient, data.IdCoupon, data.CreationDate, data.UpdatedDate,
			items, subtotal, discounts, taxes, total,
			data.Coupon is null ? null : new CartCouponDto(data.Coupon.IdCoupon, data.Coupon.Name, data.Coupon.Token, data.Coupon.Porcentage));
	}

	public static decimal CouponDiscount(CartData data) {
		var subtotal = data.Items.Sum(i => Math.Round(i.DiscountedPrice * i.Quantity, 2));
		return data.Coupon is null ? 0m : Math.Round(subtotal * data.Coupon.Porcentage / 100m, 2);
	}
}

public record CartValidationIssueDto(
	int IdProduct,
	string Name,
	string IssueType,
	string Message,
	int RequestedQuantity,
	int? AvailableStock,
	decimal? OldPrice,
	decimal? NewPrice
);

public record CartValidationDto(bool Valid, List<CartValidationIssueDto> Issues);

public record ApplyCouponResultDto(CartDto Cart, decimal DiscountApplied);
