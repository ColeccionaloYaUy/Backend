using ColeccionaloYa.Domain.Orders;
using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Domain.Payments.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.CreatePreference;

public class CreatePreferenceCommandHandler : IRequestHandler<CreatePreferenceCommand, CreatePreferenceResultDto> {
	private readonly IPaymentRepository _Repository;
	private readonly IPaymentGateway _Gateway;
	private readonly IOrderRepository _OrderRepository;

	public CreatePreferenceCommandHandler(IPaymentRepository repository, IPaymentGateway gateway, IOrderRepository orderRepository) {
		_Repository = repository;
		_Gateway = gateway;
		_OrderRepository = orderRepository;
	}

	public async Task<CreatePreferenceResultDto> Handle(CreatePreferenceCommand request, CancellationToken cancellationToken) {
		var order = await _OrderRepository.GetEntityAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);

		if (!request.IsAdmin && order.ClientId != request.RequesterId) {
			throw new OrderAccessDeniedException();
		}

		if (order.Status == OrderStatus.Cancelled || await _Repository.HasApprovedAsync(order.Id, cancellationToken)) {
			throw new OrderAlreadyPaidException(order.Id);
		}

		GatewayPreference preference;
		try {
			preference = await _Gateway.CreatePreferenceAsync(order.Id, order.Total, "UYU", cancellationToken);
		} catch (Exception ex) {
			throw new PaymentGatewayException(ex.Message);
		}

		var payment = Payment.Create(order.Id, order.Total, "UYU", _Gateway.ProviderName, preference.PreferenceId);
		await _Repository.CreateAsync(payment, cancellationToken);

		return new CreatePreferenceResultDto(preference.PreferenceId, preference.InitPoint, payment.Id);
	}
}
