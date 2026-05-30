using ColeccionaloYa.Persistence.Products.ReadModels;

namespace ColeccionaloYa.Application.Products;

public record DiscountInfoDto(
	int IdDiscount,
	decimal Porcentage,
	DateTime? ValidFrom,
	DateTime? ValidUntil,
	decimal FinalPrice
) {
	public static DiscountInfoDto From(ActiveDiscountInfo d) =>
		new(d.IdDiscount, d.Porcentage, d.ValidFrom, d.ValidUntil, d.FinalPrice);
}
