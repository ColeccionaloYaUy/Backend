namespace ColeccionaloYa.Domain.Payments;

public enum PaymentStatus {
	Pending,
	Approved,
	Rejected,
	Refunded,
	Cancelled,
	InProcess,
}

public static class PaymentStatusDb {
	public static string ToDb(PaymentStatus status) => status switch {
		PaymentStatus.InProcess => "in_process",
		_ => status.ToString().ToLowerInvariant(),
	};

	public static PaymentStatus FromDb(string value) => value switch {
		"in_process" => PaymentStatus.InProcess,
		_ => Enum.Parse<PaymentStatus>(value, ignoreCase: true),
	};
}
