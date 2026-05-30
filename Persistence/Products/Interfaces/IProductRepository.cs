using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Persistence.Products.ReadModels;
using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Persistence.Products.Interfaces;

public interface IProductRepository {
	Task<PagedData<ProductCatalogItem>> GetCatalogAsync(ProductsFilter filter, CancellationToken cancellationToken);
	Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<ProductDetailData?> GetDetailAsync(int id, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
	Task<string?> GetTypeAsync(int id, CancellationToken cancellationToken);
	Task<bool> IsInUseAsync(int id, CancellationToken cancellationToken);
	Task CreateAsync(Product product, CancellationToken cancellationToken);
	Task UpdateAsync(Product product, CancellationToken cancellationToken);
	Task LogicalDeleteAsync(int id, CancellationToken cancellationToken);
}
