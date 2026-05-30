using ColeccionaloYa.Domain.Discounts;

namespace ColeccionaloYa.Persistence.Discounts.ReadModels;

public record DiscountWithProduct(Discount Discount, string ProductName);
