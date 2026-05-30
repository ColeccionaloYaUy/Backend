namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Payments;

public class CreatePreferenceRequest {
	public int IdOrder { get; set; }
}

public class RefundPaymentRequest {
	public decimal? Amount { get; set; }
	public string? Reason { get; set; }
}

public class CancelPaymentRequest {
	public string? Reason { get; set; }
}

public class PaymentWebhookRequest {
	public string? Type { get; set; }
	public PaymentWebhookData? Data { get; set; }
}

public class PaymentWebhookData {
	public string? Id { get; set; }
}
