using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Domain.Payments.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.SyncPayment;

public class SyncPaymentCommandHandler : IRequestHandler<SyncPaymentCommand, SyncResultDto> {
	private readonly ICConnection _Connection;
	private readonly IPaymentRepository _Repository;
	private readonly IPaymentGateway _Gateway;
	private readonly IOrderRepository _OrderRepository;

	public SyncPaymentCommandHandler(ICConnection connection, IPaymentRepository repository, IPaymentGateway gateway, IOrderRepository orderRepository) {
		_Connection = connection;
		_Repository = repository;
		_Gateway = gateway;
		_OrderRepository = orderRepository;
	}

	public async Task<SyncResultDto> Handle(SyncPaymentCommand request, CancellationToken cancellationToken) {
		var payment = await _Repository.GetByIdAsync(request.IdPayment, cancellationToken)
			?? throw new PaymentNotFoundException(request.IdPayment);

		var previousStatus = payment.Status;
		var externalId = payment.ExternalId ?? payment.PreferenceId ?? payment.Id.ToString();

		Persistence.Payments.Interfaces.GatewayPaymentStatus gatewayStatus;
		try {
			gatewayStatus = await _Gateway.GetPaymentStatusAsync(externalId, cancellationToken);
		} catch (Exception ex) {
			throw new PaymentGatewayException(ex.Message);
		}

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

		var changed = previousStatus != payment.Status;
		return new SyncResultDto(payment.Id, PaymentStatusDb.ToDb(payment.Status), payment.ExternalId, payment.UpdatedDate, changed);
	}
}
