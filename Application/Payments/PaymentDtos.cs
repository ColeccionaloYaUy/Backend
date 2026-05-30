using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Persistence.Payments.ReadModels;

namespace ColeccionaloYa.Application.Payments;

public record PaymentDto(
	int IdPayment,
	int IdOrder,
	string? ExternalId,
	string? PreferenceId,
	string Status,
	decimal Amount,
	string Currency,
	string? PaymentMethod,
	string? Provider,
	DateTime CreationDate,
	DateTime UpdatedDate
) {
	public static PaymentDto From(Payment p) =>
		new(p.Id, p.OrderId, p.ExternalId, p.PreferenceId, PaymentStatusDb.ToDb(p.Status), p.Amount, p.Currency,
			p.PaymentMethod, p.Provider, p.CreationDate, p.UpdatedDate);
}

public record PaymentListItemDto(
	int IdPayment,
	int IdOrder,
	string? ExternalId,
	string? PreferenceId,
	string Status,
	decimal Amount,
	string Currency,
	string? PaymentMethod,
	string? Provider,
	DateTime CreationDate,
	DateTime UpdatedDate,
	string ClientEmail
) {
	public static PaymentListItemDto From(PaymentListItem p) =>
		new(p.Payment.Id, p.Payment.OrderId, p.Payment.ExternalId, p.Payment.PreferenceId,
			PaymentStatusDb.ToDb(p.Payment.Status), p.Payment.Amount, p.Payment.Currency,
			p.Payment.PaymentMethod, p.Payment.Provider, p.Payment.CreationDate, p.Payment.UpdatedDate, p.ClientEmail);
}

// Admin-only detail: includes raw_response (infrastructure data, never exposed to role User).
public record PaymentAdminDto(
	int IdPayment,
	int IdOrder,
	string? ExternalId,
	string? PreferenceId,
	string Status,
	decimal Amount,
	string Currency,
	string? PaymentMethod,
	string? Provider,
	DateTime CreationDate,
	DateTime UpdatedDate,
	string? RawResponse
) {
	public static PaymentAdminDto From(Payment p) =>
		new(p.Id, p.OrderId, p.ExternalId, p.PreferenceId, PaymentStatusDb.ToDb(p.Status), p.Amount, p.Currency,
			p.PaymentMethod, p.Provider, p.CreationDate, p.UpdatedDate, p.RawResponse);
}

public record PaymentOrderBriefDto(
	int IdOrder,
	DateOnly CreationDate,
	string Status,
	decimal Total,
	int IdClient,
	string ClientName,
	string ClientEmail
);

public record PaymentDetailDto(PaymentAdminDto Payment, PaymentOrderBriefDto Order) {
	public static PaymentDetailDto From(PaymentDetailData data) =>
		new(
			PaymentAdminDto.From(data.Payment),
			new PaymentOrderBriefDto(data.Order.IdOrder, data.Order.CreationDate, data.Order.Status, data.Order.Total,
				data.Order.IdClient, data.Order.ClientName, data.Order.ClientEmail));
}

public record CreatePreferenceResultDto(string PreferenceId, string InitPoint, int IdPayment);

public record PaymentStatusResultDto(string Status, PaymentDto? PaymentDetails);

public record RefundResultDto(int IdPayment, string Status, decimal Amount, decimal RefundedAmount, DateTime UpdatedDate);

public record CancelResultDto(int IdPayment, string Status, DateTime UpdatedDate);

public record SyncResultDto(int IdPayment, string Status, string? ExternalId, DateTime UpdatedDate, bool Changed);
