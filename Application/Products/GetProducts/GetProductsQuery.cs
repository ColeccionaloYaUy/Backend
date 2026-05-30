using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Products.GetProducts;

public record GetProductsQuery(
	int Page,
	int PageSize,
	string? Search,
	string? Type,
	string? TypeBook,
	IReadOnlyList<int> Tags,
	IReadOnlyList<int> Genres,
	IReadOnlyList<int> Authors,
	int? Franchise,
	decimal? MinPrice,
	decimal? MaxPrice,
	bool? InStock,
	bool? OnSale,
	bool? Featured,
	bool NewArrivals,
	int? RelatedTo,
	string Sort
) : IRequest<PagedResult<ProductSummaryDto>>, IPagedQuery;
