using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Stock.GetProductStockMovements;

public record GetProductStockMovementsQuery(int ProductId, int Page, int PageSize, DateOnly? DateFrom, DateOnly? DateTo)
	: IRequest<PagedResult<StockMovementHistoryDto>>, IPagedQuery;
