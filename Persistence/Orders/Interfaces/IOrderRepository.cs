using ColeccionaloYa.Domain.Orders;
using ColeccionaloYa.Persistence.Orders.ReadModels;
using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Persistence.Orders.Interfaces;

public interface IOrderRepository {
	Task<PagedData<OrderListItem>> GetPagedAsync(int page, int pageSize, string? status, DateOnly? dateFrom, DateOnly? dateTo, int? clientId, CancellationToken cancellationToken);
	Task<int?> GetOwnerClientIdAsync(int orderId, CancellationToken cancellationToken);
	Task<OrderDetailData?> GetDetailAsync(int orderId, CancellationToken cancellationToken);
	Task<OrderTrackingData?> GetTrackingAsync(int orderId, CancellationToken cancellationToken);
	Task<Order?> GetEntityAsync(int orderId, CancellationToken cancellationToken);
	Task<bool> AddressBelongsToClientAsync(int addressId, int clientId, CancellationToken cancellationToken);
	Task<int> CreateAddressSnapshotAsync(int addressId, CancellationToken cancellationToken);
	Task CreateOrderAsync(Order order, CancellationToken cancellationToken);
	Task CreateOrderLinesAsync(int orderId, IReadOnlyCollection<OrderLine> lines, CancellationToken cancellationToken);
	Task AddStatusHistoryAsync(int orderId, OrderStatus status, string? reason, int? changedBy, CancellationToken cancellationToken);
	Task UpdateStatusAsync(Order order, CancellationToken cancellationToken);
	Task UpdateTrackingAsync(int orderId, string tracking, CancellationToken cancellationToken);
	Task<List<(int ProductId, int Quantity)>> GetLineQuantitiesAsync(int orderId, CancellationToken cancellationToken);
}
