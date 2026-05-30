using MediatR;

namespace ColeccionaloYa.Application.Orders.CreateOrder;

public record CreateOrderCommand(
	int ClientId,
	int DeliveryAddressId,
	int OrderAddressId,
	string? Observations
) : IRequest<OrderDetailDto>;
