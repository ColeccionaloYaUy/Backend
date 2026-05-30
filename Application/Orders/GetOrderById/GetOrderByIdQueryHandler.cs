using ColeccionaloYa.Domain.Orders.Exceptions;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Orders.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto> {
	private readonly IOrderRepository _Repository;

	public GetOrderByIdQueryHandler(IOrderRepository repository) {
		_Repository = repository;
	}

	public async Task<OrderDetailDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken) {
		await OrderAccessGuard.EnsureAccessAsync(_Repository, request.OrderId, request.RequesterId, request.IsAdmin, cancellationToken);

		var data = await _Repository.GetDetailAsync(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);
		return OrderDetailDto.From(data);
	}
}
