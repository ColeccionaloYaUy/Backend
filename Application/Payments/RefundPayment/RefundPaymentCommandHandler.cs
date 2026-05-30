using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Domain.Payments.Exceptions;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.RefundPayment;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, RefundResultDto> {
	private readonly IPaymentRepository _Repository;
	private readonly IPaymentGateway _Gateway;

	public RefundPaymentCommandHandler(IPaymentRepository repository, IPaymentGateway gateway) {
		_Repository = repository;
		_Gateway = gateway;
	}

	public async Task<RefundResultDto> Handle(RefundPaymentCommand request, CancellationToken cancellationToken) {
		var payment = await _Repository.GetByIdAsync(request.IdPayment, cancellationToken)
			?? throw new PaymentNotFoundException(request.IdPayment);

		// Validate state/amount before contacting the gateway.
		payment.Refund(request.Amount);

		try {
			await _Gateway.RefundAsync(payment.ExternalId ?? payment.PreferenceId ?? string.Empty, request.Amount, cancellationToken);
		} catch (Exception ex) {
			throw new PaymentGatewayException(ex.Message);
		}

		await _Repository.UpdateAsync(payment, cancellationToken);

		var refunded = request.Amount ?? payment.Amount;
		return new RefundResultDto(payment.Id, PaymentStatusDb.ToDb(payment.Status), payment.Amount, refunded, payment.UpdatedDate);
	}
}
