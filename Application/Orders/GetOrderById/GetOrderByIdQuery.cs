using MediatR;

namespace ColeccionaloYa.Application.Orders.GetOrderById;

public record GetOrderByIdQuery(int OrderId, int RequesterId, bool IsAdmin) : IRequest<OrderDetailDto>;
