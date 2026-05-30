using ColeccionaloYa.Domain.Coupons;
using ColeccionaloYa.Persistence.Coupons.ReadModels;
using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Persistence.Coupons.Interfaces;

public interface ICouponRepository {
	Task<PagedData<CouponWithCount>> GetPagedAsync(int page, int pageSize, bool? isActive, string? search, CancellationToken cancellationToken);
	Task<Coupon?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<CouponWithClients?> GetWithClientsAsync(int id, CancellationToken cancellationToken);
	Task<Coupon?> GetByTokenAsync(string token, CancellationToken cancellationToken);
	Task<List<Coupon>> GetForClientAsync(int clientId, CancellationToken cancellationToken);
	Task<bool> ExistsByTokenAsync(string token, int? excludeId, CancellationToken cancellationToken);
	Task<bool> IsAssignedAsync(int couponId, int clientId, CancellationToken cancellationToken);
	Task<bool> ClientsExistAsync(IReadOnlyCollection<int> clientIds, CancellationToken cancellationToken);
	Task<bool> AnyClientAssignedAsync(int couponId, IReadOnlyCollection<int> clientIds, CancellationToken cancellationToken);
	Task<bool> ClientAssignmentExistsAsync(int couponId, int clientId, CancellationToken cancellationToken);
	Task<List<CouponClientBrief>> GetAssignedClientsAsync(int couponId, CancellationToken cancellationToken);
	Task CreateAsync(Coupon coupon, CancellationToken cancellationToken);
	Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken);
	Task LogicalDeleteAsync(int id, CancellationToken cancellationToken);
	Task AssignClientsAsync(int couponId, IReadOnlyCollection<int> clientIds, CancellationToken cancellationToken);
	Task RemoveClientAsync(int couponId, int clientId, CancellationToken cancellationToken);
}
