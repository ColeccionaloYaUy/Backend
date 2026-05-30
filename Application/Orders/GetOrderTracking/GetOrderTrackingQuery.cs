using MediatR;

namespace ColeccionaloYa.Application.Orders.GetOrderTracking;

public record GetOrderTrackingQuery(int OrderId, int RequesterId, bool IsAdmin) : IRequest<OrderTrackingDto>;
