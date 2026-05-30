using ColeccionaloYa.Application.Orders;
using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.GetOrderPaymentStatus;

public class GetOrderPaymentStatusQueryHandler : IRequestHandler<GetOrderPaymentStatusQuery, PaymentStatusResultDto> {
	private readonly IPaymentRepository _Repository;
	private readonly IOrderRepository _OrderRepository;

	public GetOrderPaymentStatusQueryHandler(IPaymentRepository repository, IOrderRepository orderRepository) {
		_Repository = repository;
		_OrderRepository = orderRepository;
	}

	public async Task<PaymentStatusResultDto> Handle(GetOrderPaymentStatusQuery request, CancellationToken cancellationToken) {
		var ownerId = await _OrderRepository.GetOwnerClientIdAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);

		if (!request.IsAdmin && ownerId != request.RequesterId) {
			throw new OrderAccessDeniedException();
		}

		var last = await _Repository.GetLastByOrderAsync(request.OrderId, cancellationToken);
		if (last is null) {
			return new PaymentStatusResultDto("no_payment", null);
		}

		var dto = PaymentDto.From(last);
		return new PaymentStatusResultDto(dto.Status, dto);
	}
}
