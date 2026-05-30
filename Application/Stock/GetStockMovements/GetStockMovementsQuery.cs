using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Stock.GetStockMovements;

public record GetStockMovementsQuery(int Page, int PageSize, int? ProductId, DateOnly? DateFrom, DateOnly? DateTo)
	: IRequest<PagedResult<StockMovementDto>>, IPagedQuery;
