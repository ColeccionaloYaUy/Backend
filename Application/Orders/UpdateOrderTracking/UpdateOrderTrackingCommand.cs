using MediatR;

namespace ColeccionaloYa.Application.Orders.UpdateOrderTracking;

public record UpdateOrderTrackingCommand(int OrderId, string Tracking) : IRequest<OrderDetailDto>;
