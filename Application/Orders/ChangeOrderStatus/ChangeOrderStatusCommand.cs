using MediatR;

namespace ColeccionaloYa.Application.Orders.ChangeOrderStatus;

public record ChangeOrderStatusCommand(int OrderId, string Status, string? Reason, int ChangedBy) : IRequest<OrderDetailDto>;
