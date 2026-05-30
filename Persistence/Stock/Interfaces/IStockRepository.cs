using ColeccionaloYa.Domain.Stock;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Persistence.Stock.ReadModels;

namespace ColeccionaloYa.Persistence.Stock.Interfaces;

public interface IStockRepository {
	Task<PagedData<StockMovementWithProduct>> GetPagedAsync(int page, int pageSize, int? productId, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken);
	Task<StockSummary> GetSummaryAsync(int productId, CancellationToken cancellationToken);
	Task<int> GetCurrentStockAsync(int productId, CancellationToken cancellationToken);
	Task<PagedData<StockMovementHistoryItem>> GetMovementsAsync(int productId, int page, int pageSize, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken);
	Task<bool> OrderExistsAsync(int orderId, CancellationToken cancellationToken);
	Task CreateAsync(StockMovement movement, CancellationToken cancellationToken);
}
