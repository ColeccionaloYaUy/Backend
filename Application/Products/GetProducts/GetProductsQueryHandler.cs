using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Products.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductSummaryDto>> {
	private readonly IProductRepository _Repository;

	public GetProductsQueryHandler(IProductRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<ProductSummaryDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken) {
		var filter = new ProductsFilter(
			request.Page,
			request.PageSize,
			request.Search,
			request.Type,
			request.TypeBook,
			request.Tags,
			request.Genres,
			request.Authors,
			request.Franchise,
			request.MinPrice,
			request.MaxPrice,
			request.InStock,
			request.OnSale,
			request.Featured,
			request.NewArrivals,
			request.RelatedTo,
			request.Sort);

		var data = await _Repository.GetCatalogAsync(filter, cancellationToken);
		return data.ToPagedResult(request.PageSize, ProductSummaryDto.From);
	}
}
