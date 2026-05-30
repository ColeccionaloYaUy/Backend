namespace ColeccionaloYa.Domain.Carts;

/// <summary>
/// Tax policy for cart/order totals. The base currency is UYU and the applicable VAT (IVA) rate is 22%.
/// </summary>
public static class CartTax {
	public const decimal Rate = 0.22m;
}
