using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Orders.GetOrders;

public record GetOrdersQuery(int Page, int PageSize, string? Status, DateOnly? DateFrom, DateOnly? DateTo, int? ClientId)
	: IRequest<PagedResult<OrderListItemDto>>, IPagedQuery;
