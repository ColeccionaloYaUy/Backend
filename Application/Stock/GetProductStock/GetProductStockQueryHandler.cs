using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Stock.GetProductStock;

public class GetProductStockQueryHandler : IRequestHandler<GetProductStockQuery, StockSummaryDto> {
	private readonly IStockRepository _Repository;
	private readonly IProductRepository _ProductRepository;

	public GetProductStockQueryHandler(IStockRepository repository, IProductRepository productRepository) {
		_Repository = repository;
		_ProductRepository = productRepository;
	}

	public async Task<StockSummaryDto> Handle(GetProductStockQuery request, CancellationToken cancellationToken) {
		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		var summary = await _Repository.GetSummaryAsync(request.ProductId, cancellationToken);
		return StockSummaryDto.From(summary);
	}
}
