using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Domain.Payments.Exceptions;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.CancelPayment;

public class CancelPaymentCommandHandler : IRequestHandler<CancelPaymentCommand, CancelResultDto> {
	private readonly IPaymentRepository _Repository;
	private readonly IPaymentGateway _Gateway;

	public CancelPaymentCommandHandler(IPaymentRepository repository, IPaymentGateway gateway) {
		_Repository = repository;
		_Gateway = gateway;
	}

	public async Task<CancelResultDto> Handle(CancelPaymentCommand request, CancellationToken cancellationToken) {
		var payment = await _Repository.GetByIdAsync(request.IdPayment, cancellationToken)
			?? throw new PaymentNotFoundException(request.IdPayment);

		payment.Cancel();

		try {
			await _Gateway.CancelAsync(payment.ExternalId ?? payment.PreferenceId ?? string.Empty, cancellationToken);
		} catch (Exception ex) {
			throw new PaymentGatewayException(ex.Message);
		}

		await _Repository.UpdateAsync(payment, cancellationToken);
		return new CancelResultDto(payment.Id, PaymentStatusDb.ToDb(payment.Status), payment.UpdatedDate);
	}
}
