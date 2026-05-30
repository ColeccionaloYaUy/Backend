using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.ProcessWebhook;

public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand, Unit> {
	private readonly ICConnection _Connection;
	private readonly IPaymentRepository _Repository;
	private readonly IPaymentGateway _Gateway;
	private readonly IOrderRepository _OrderRepository;

	public ProcessWebhookCommandHandler(ICConnection connection, IPaymentRepository repository, IPaymentGateway gateway, IOrderRepository orderRepository) {
		_Connection = connection;
		_Repository = repository;
		_Gateway = gateway;
		_OrderRepository = orderRepository;
	}

	public async Task<Unit> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken) {
		if (string.IsNullOrWhiteSpace(request.ExternalId)) {
			return Unit.Value;
		}

		var payment = await _Repository.GetByExternalIdAsync(request.ExternalId, cancellationToken);
		if (payment is null) {
			// Unknown external id: acknowledge without side effects (provider may retry / send unrelated events).
			return Unit.Value;
		}

		var gatewayStatus = await _Gateway.GetPaymentStatusAsync(request.ExternalId, cancellationToken);
		payment.ApplyGatewayUpdate(gatewayStatus.Status, gatewayStatus.ExternalId, gatewayStatus.PaymentMethod, gatewayStatus.RawResponse);

		await _Connection.BeginTransaction(cancellationToken);
		try {
			await _Repository.UpdateAsync(payment, cancellationToken);
			if (payment.Status == PaymentStatus.Approved) {
				await PaymentOrderConfirmer.ConfirmIfPendingAsync(_OrderRepository, payment.OrderId, cancellationToken);
			}
			await _Connection.CommitTransaction(cancellationToken);
		} catch {
			await _Connection.CancelTransaction(cancellationToken);
			throw;
		}

		return Unit.Value;
	}
}
