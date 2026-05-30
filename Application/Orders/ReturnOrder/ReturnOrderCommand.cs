using MediatR;

namespace ColeccionaloYa.Application.Orders.ReturnOrder;

public record ReturnOrderCommand(int OrderId, int RequesterId, bool IsAdmin, string Reason) : IRequest<OrderDetailDto>;
