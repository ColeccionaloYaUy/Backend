using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Stock.GetStockMovements;

public class GetStockMovementsQueryHandler : IRequestHandler<GetStockMovementsQuery, PagedResult<StockMovementDto>> {
	private readonly IStockRepository _Repository;

	public GetStockMovementsQueryHandler(IStockRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<StockMovementDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetPagedAsync(request.Page, request.PageSize, request.ProductId, request.DateFrom, request.DateTo, cancellationToken);
		return data.ToPagedResult(request.PageSize, StockMovementDto.From);
	}
}
