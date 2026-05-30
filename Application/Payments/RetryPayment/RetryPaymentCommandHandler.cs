using ColeccionaloYa.Application.Payments.CreatePreference;
using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Domain.Payments.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.RetryPayment;

public class RetryPaymentCommandHandler : IRequestHandler<RetryPaymentCommand, CreatePreferenceResultDto> {
	private readonly IPaymentRepository _Repository;
	private readonly IPaymentGateway _Gateway;
	private readonly IOrderRepository _OrderRepository;

	public RetryPaymentCommandHandler(IPaymentRepository repository, IPaymentGateway gateway, IOrderRepository orderRepository) {
		_Repository = repository;
		_Gateway = gateway;
		_OrderRepository = orderRepository;
	}

	public async Task<CreatePreferenceResultDto> Handle(RetryPaymentCommand request, CancellationToken cancellationToken) {
		var payment = await _Repository.GetByIdAsync(request.IdPayment, cancellationToken)
			?? throw new PaymentNotFoundException(request.IdPayment);

		var ownerId = await _OrderRepository.GetOwnerClientIdAsync(payment.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(payment.OrderId);
		if (!request.IsAdmin && ownerId != request.RequesterId) {
			throw new OrderAccessDeniedException();
		}

		payment.EnsureRetryable();

		var order = await _OrderRepository.GetEntityAsync(payment.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(payment.OrderId);

		GatewayPreference preference;
		try {
			preference = await _Gateway.CreatePreferenceAsync(order.Id, order.Total, payment.Currency, cancellationToken);
		} catch (Exception ex) {
			throw new PaymentGatewayException(ex.Message);
		}

		var newPayment = Payment.Create(order.Id, order.Total, payment.Currency, _Gateway.ProviderName, preference.PreferenceId);
		await _Repository.CreateAsync(newPayment, cancellationToken);

		return new CreatePreferenceResultDto(preference.PreferenceId, preference.InitPoint, newPayment.Id);
	}
}
