using ColeccionaloYa.Domain.Payments.Exceptions;

namespace ColeccionaloYa.Domain.Payments;

public class Payment {
	public int Id { get; internal set; }
	public int OrderId { get; internal set; }
	public string? ExternalId { get; internal set; }
	public string? PreferenceId { get; internal set; }
	public PaymentStatus Status { get; internal set; }
	public decimal Amount { get; internal set; }
	public string Currency { get; internal set; } = "UYU";
	public string? PaymentMethod { get; internal set; }
	public string? Provider { get; internal set; }
	public DateTime CreationDate { get; internal set; }
	public DateTime UpdatedDate { get; internal set; }
	public string? RawResponse { get; internal set; }

	internal Payment() { }

	public static Payment Create(int orderId, decimal amount, string currency, string? provider, string? preferenceId) {
		var now = DateTime.UtcNow;
		return new Payment {
			OrderId = orderId,
			Amount = amount,
			Currency = string.IsNullOrWhiteSpace(currency) ? "UYU" : currency,
			Provider = provider,
			PreferenceId = preferenceId,
			Status = PaymentStatus.Pending,
			CreationDate = now,
			UpdatedDate = now,
		};
	}

	public void ApplyGatewayUpdate(PaymentStatus status, string? externalId, string? paymentMethod, string? rawResponse) {
		Status = status;
		if (externalId is not null) ExternalId = externalId;
		if (paymentMethod is not null) PaymentMethod = paymentMethod;
		if (rawResponse is not null) RawResponse = rawResponse;
		UpdatedDate = DateTime.UtcNow;
	}

	public void Refund(decimal? amount) {
		if (Status != PaymentStatus.Approved) {
			throw new PaymentNotRefundableException(Id);
		}
		if (amount.HasValue && amount.Value > Amount) {
			throw new InvalidRefundAmountException(Id);
		}
		Status = PaymentStatus.Refunded;
		UpdatedDate = DateTime.UtcNow;
	}

	public void Cancel() {
		if (Status is not (PaymentStatus.Pending or PaymentStatus.InProcess)) {
			throw new PaymentNotCancellableException(Id);
		}
		Status = PaymentStatus.Cancelled;
		UpdatedDate = DateTime.UtcNow;
	}

	public void EnsureRetryable() {
		if (Status is not (PaymentStatus.Rejected or PaymentStatus.Cancelled)) {
			throw new PaymentNotRetryableException(Id);
		}
	}

	public void AssignId(int id) {
		Id = id;
	}
}
