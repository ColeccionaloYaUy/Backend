using ColeccionaloYa.Application.Orders;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.GetOrderPayments;

public class GetOrderPaymentsQueryHandler : IRequestHandler<GetOrderPaymentsQuery, List<PaymentDto>> {
	private readonly IPaymentRepository _Repository;
	private readonly IOrderRepository _OrderRepository;

	public GetOrderPaymentsQueryHandler(IPaymentRepository repository, IOrderRepository orderRepository) {
		_Repository = repository;
		_OrderRepository = orderRepository;
	}

	public async Task<List<PaymentDto>> Handle(GetOrderPaymentsQuery request, CancellationToken cancellationToken) {
		await OrderAccessGuard.EnsureAccessAsync(_OrderRepository, request.OrderId, request.RequesterId, request.IsAdmin, cancellationToken);

		var payments = await _Repository.GetByOrderAsync(request.OrderId, cancellationToken);
		return payments.Select(PaymentDto.From).ToList();
	}
}
