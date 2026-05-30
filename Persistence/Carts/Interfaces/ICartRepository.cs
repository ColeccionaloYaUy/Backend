using ColeccionaloYa.Persistence.Carts.ReadModels;

namespace ColeccionaloYa.Persistence.Carts.Interfaces;

public interface ICartRepository {
	Task<int> EnsureCartAsync(int clientId, CancellationToken cancellationToken);
	Task<int?> GetCartIdAsync(int clientId, CancellationToken cancellationToken);
	Task<CartData?> GetCartDataAsync(int clientId, CancellationToken cancellationToken);
	Task<bool> ItemExistsAsync(int cartId, int productId, CancellationToken cancellationToken);
	Task<int?> GetItemQuantityAsync(int cartId, int productId, CancellationToken cancellationToken);
	Task InsertItemAsync(int cartId, int productId, int quantity, CancellationToken cancellationToken);
	Task SetItemQuantityAsync(int cartId, int productId, int quantity, CancellationToken cancellationToken);
	Task RemoveItemAsync(int cartId, int productId, CancellationToken cancellationToken);
	Task ClearItemsAsync(int cartId, CancellationToken cancellationToken);
	Task SetCouponAsync(int cartId, int? couponId, CancellationToken cancellationToken);
	Task TouchAsync(int cartId, CancellationToken cancellationToken);
	Task<decimal> GetNetSubtotalAsync(int clientId, CancellationToken cancellationToken);
}
