using ColeccionaloYa.Domain.Discounts;
using ColeccionaloYa.Persistence.Discounts.ReadModels;
using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Persistence.Discounts.Interfaces;

public interface IDiscountRepository {
	Task<PagedData<DiscountWithProduct>> GetPagedAsync(int page, int pageSize, bool? isActive, int? productId, CancellationToken cancellationToken);
	Task<DiscountWithProduct?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<Discount?> GetEntityAsync(int id, CancellationToken cancellationToken);
	Task<bool> HasOverlappingActiveAsync(int productId, DateTime? validFrom, DateTime? validUntil, int? excludeId, CancellationToken cancellationToken);
	Task CreateAsync(Discount discount, CancellationToken cancellationToken);
	Task UpdateAsync(Discount discount, CancellationToken cancellationToken);
}
