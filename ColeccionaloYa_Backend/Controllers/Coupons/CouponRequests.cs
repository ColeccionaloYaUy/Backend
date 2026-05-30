using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Coupons;

public class CreateCouponRequest {
	[Required]
	[MaxLength(20)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[MaxLength(10)]
	public string Token { get; set; } = string.Empty;

	[Required]
	public string Description { get; set; } = string.Empty;

	public DateTime? ValidFrom { get; set; }
	public DateTime? ValidUntil { get; set; }

	[Range(0, 100)]
	public decimal Porcentage { get; set; }
}

public class UpdateCouponRequest {
	[MaxLength(20)]
	public string? Name { get; set; }

	public string? Description { get; set; }
	public DateTime? ValidFrom { get; set; }
	public DateTime? ValidUntil { get; set; }

	[Range(0, 100)]
	public decimal? Porcentage { get; set; }
}

public class ValidateCouponRequest {
	[Required]
	public string Token { get; set; } = string.Empty;
}

public class AssignCouponClientsRequest {
	[Required]
	[MinLength(1)]
	public List<int> ClientIds { get; set; } = new();
}
