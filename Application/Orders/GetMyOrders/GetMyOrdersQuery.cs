using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Orders.GetMyOrders;

public record GetMyOrdersQuery(int ClientId, int Page, int PageSize, string? Status)
	: IRequest<PagedResult<OrderListItemDto>>, IPagedQuery;
