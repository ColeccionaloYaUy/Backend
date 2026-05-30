namespace ColeccionaloYa.Persistence.Products.ReadModels;

public record ActiveDiscountInfo(
	int IdDiscount,
	decimal Porcentage,
	DateTime? ValidFrom,
	DateTime? ValidUntil,
	decimal FinalPrice
);
