using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Payments.Services;

/// <summary>
/// Simulated payment gateway used while the real provider integration (MercadoPago SDK + credentials)
/// is not configured. It produces deterministic, self-consistent responses so the full payment flow
/// can be exercised end to end. Replace with a real <see cref="IPaymentGateway"/> implementation in production.
/// </summary>
[Injectable(ServiceLifetime.Singleton)]
public class FakePaymentGateway : IPaymentGateway {
	public string ProviderName => "fake";

	public Task<GatewayPreference> CreatePreferenceAsync(int orderId, decimal amount, string currency, CancellationToken cancellationToken) {
		var preferenceId = $"pref_{Guid.NewGuid():N}";
		var initPoint = $"https://payments.local/checkout/{preferenceId}";
		return Task.FromResult(new GatewayPreference(preferenceId, initPoint));
	}

	public Task<GatewayPaymentStatus> GetPaymentStatusAsync(string externalId, CancellationToken cancellationToken) {
		var raw = $"{{\"id\":\"{externalId}\",\"status\":\"approved\",\"payment_method\":\"credit_card\"}}";
		return Task.FromResult(new GatewayPaymentStatus(PaymentStatus.Approved, externalId, "credit_card", raw));
	}

	public Task RefundAsync(string externalId, decimal? amount, CancellationToken cancellationToken) => Task.CompletedTask;

	public Task CancelAsync(string externalId, CancellationToken cancellationToken) => Task.CompletedTask;
}
