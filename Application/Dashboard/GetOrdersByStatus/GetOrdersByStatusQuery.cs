using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetOrdersByStatus;

public record GetOrdersByStatusQuery(DateOnly? DateFrom, DateOnly? DateTo) : IRequest<OrdersByStatusDto>;
