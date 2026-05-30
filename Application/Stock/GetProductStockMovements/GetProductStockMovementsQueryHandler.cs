using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Stock.GetProductStockMovements;

public class GetProductStockMovementsQueryHandler : IRequestHandler<GetProductStockMovementsQuery, PagedResult<StockMovementHistoryDto>> {
	private readonly IStockRepository _Repository;
	private readonly IProductRepository _ProductRepository;

	public GetProductStockMovementsQueryHandler(IStockRepository repository, IProductRepository productRepository) {
		_Repository = repository;
		_ProductRepository = productRepository;
	}

	public async Task<PagedResult<StockMovementHistoryDto>> Handle(GetProductStockMovementsQuery request, CancellationToken cancellationToken) {
		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		var data = await _Repository.GetMovementsAsync(request.ProductId, request.Page, request.PageSize, request.DateFrom, request.DateTo, cancellationToken);
		return data.ToPagedResult(request.PageSize, StockMovementHistoryDto.From);
	}
}
