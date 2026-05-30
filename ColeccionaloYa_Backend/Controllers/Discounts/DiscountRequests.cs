using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Discounts;

public class CreateDiscountRequest {
	public int IdProduct { get; set; }

	[Range(0, 100)]
	public decimal Porcentage { get; set; }

	public DateTime? ValidFrom { get; set; }
	public DateTime? ValidUntil { get; set; }
}

public class UpdateDiscountRequest {
	[Range(0, 100)]
	public decimal? Porcentage { get; set; }

	public DateTime? ValidFrom { get; set; }
	public DateTime? ValidUntil { get; set; }
}

public class ChangeDiscountStatusRequest {
	[Required]
	public string Action { get; set; } = string.Empty;
}
