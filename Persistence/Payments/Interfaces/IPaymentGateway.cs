using ColeccionaloYa.Domain.Payments;

namespace ColeccionaloYa.Persistence.Payments.Interfaces;

public record GatewayPreference(string PreferenceId, string InitPoint);

public record GatewayPaymentStatus(PaymentStatus Status, string ExternalId, string? PaymentMethod, string RawResponse);

/// <summary>
/// Abstraction over the external payment provider (e.g. MercadoPago). The concrete production
/// implementation requires the provider SDK and credentials; this contract lets the rest of the
/// system remain provider-agnostic.
/// </summary>
public interface IPaymentGateway {
	string ProviderName { get; }
	Task<GatewayPreference> CreatePreferenceAsync(int orderId, decimal amount, string currency, CancellationToken cancellationToken);
	Task<GatewayPaymentStatus> GetPaymentStatusAsync(string externalId, CancellationToken cancellationToken);
	Task RefundAsync(string externalId, decimal? amount, CancellationToken cancellationToken);
	Task CancelAsync(string externalId, CancellationToken cancellationToken);
}
