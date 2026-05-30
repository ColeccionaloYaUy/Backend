using ColeccionaloYa.Persistence.Discounts.ReadModels;

namespace ColeccionaloYa.Application.Discounts;

public record DiscountDto(
	int IdDiscount,
	int IdProduct,
	string ProductName,
	decimal Porcentage,
	DateTime CreationDate,
	DateTime? ValidFrom,
	DateTime? ValidUntil,
	bool IsActive
) {
	public static DiscountDto From(DiscountWithProduct d) =>
		new(d.Discount.Id, d.Discount.ProductId, d.ProductName, d.Discount.Porcentage,
			d.Discount.CreationDate, d.Discount.ValidFrom, d.Discount.ValidUntil, d.Discount.IsActive);
}
