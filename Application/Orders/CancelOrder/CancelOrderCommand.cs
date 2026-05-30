using MediatR;

namespace ColeccionaloYa.Application.Orders.CancelOrder;

public record CancelOrderCommand(int OrderId, int RequesterId, bool IsAdmin, string Reason) : IRequest<OrderDetailDto>;
