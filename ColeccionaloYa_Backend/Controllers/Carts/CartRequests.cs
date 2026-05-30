using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Carts;

public class AddCartItemRequest {
	public int IdProduct { get; set; }

	[Range(1, int.MaxValue)]
	public int Quantity { get; set; }
}

public class UpdateCartItemRequest {
	[Range(1, int.MaxValue)]
	public int Quantity { get; set; }
}

public class ApplyCouponRequest {
	[Required]
	public string CouponToken { get; set; } = string.Empty;
}
