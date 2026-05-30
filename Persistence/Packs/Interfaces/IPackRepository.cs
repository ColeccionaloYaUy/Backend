using ColeccionaloYa.Domain.Packs;
using ColeccionaloYa.Persistence.Packs.ReadModels;

namespace ColeccionaloYa.Persistence.Packs.Interfaces;

public interface IPackRepository {
	Task<bool> ExistsAsync(int productId, CancellationToken cancellationToken);
	Task<PackDetailData?> GetDetailAsync(int productId, CancellationToken cancellationToken);
	Task<bool> AllValidNonPackProductsAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
	Task<bool> IsValidNonPackProductAsync(int productId, CancellationToken cancellationToken);
	Task<bool> ItemExistsAsync(int packId, int productId, CancellationToken cancellationToken);
	Task CreateAsync(Pack pack, CancellationToken cancellationToken);
	Task AddItemsAsync(int packId, IReadOnlyCollection<(int ProductId, int Quantity)> items, CancellationToken cancellationToken);
	Task AddItemAsync(int packId, int productId, int quantity, CancellationToken cancellationToken);
	Task UpdateItemAsync(int packId, int productId, int quantity, CancellationToken cancellationToken);
	Task RemoveItemAsync(int packId, int productId, CancellationToken cancellationToken);
	Task RecalculateCountAsync(int packId, CancellationToken cancellationToken);
}
