using ColeccionaloYa.Domain.Payments;

namespace ColeccionaloYa.Persistence.Payments.ReadModels;

public record PaymentListItem(Payment Payment, string ClientEmail);

public record PaymentOrderBrief(
	int IdOrder,
	DateOnly CreationDate,
	string Status,
	decimal Total,
	int IdClient,
	string ClientName,
	string ClientEmail
);

public record PaymentDetailData(Payment Payment, PaymentOrderBrief Order);
