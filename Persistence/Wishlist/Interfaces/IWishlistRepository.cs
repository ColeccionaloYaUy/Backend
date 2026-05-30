using ColeccionaloYa.Persistence.Wishlist.ReadModels;

namespace ColeccionaloYa.Persistence.Wishlist.Interfaces;

public interface IWishlistRepository {
	Task<List<WishlistProductItem>> GetByClientAsync(int clientId, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(int clientId, int productId, CancellationToken cancellationToken);
	Task AddAsync(int clientId, int productId, CancellationToken cancellationToken);
	Task RemoveAsync(int clientId, int productId, CancellationToken cancellationToken);
}
