using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Orders;

public class CreateOrderRequest {
	public int IdDeliveryAddress { get; set; }

	public int IdOrderAddress { get; set; }

	[MaxLength(200)]
	public string? Observations { get; set; }
}

public class ChangeOrderStatusRequest {
	[Required]
	public string Status { get; set; } = string.Empty;

	[MaxLength(500)]
	public string? Reason { get; set; }
}

public class CancelOrderRequest {
	[Required]
	[MaxLength(500)]
	public string Reason { get; set; } = string.Empty;
}

public class UpdateOrderTrackingRequest {
	[Required]
	[MaxLength(20)]
	public string Tracking { get; set; } = string.Empty;
}

public class ReturnOrderRequest {
	[Required]
	[MaxLength(500)]
	public string Reason { get; set; } = string.Empty;
}
