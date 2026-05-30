using ColeccionaloYa.Persistence.Dashboard.ReadModels;

namespace ColeccionaloYa.Persistence.Dashboard.Interfaces;

public interface IDashboardRepository {
	Task<SummaryTotals> GetSummaryAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
	Task<List<SalesBucket>> GetSalesSeriesAsync(string period, DateOnly from, DateOnly to, string statusFilter, CancellationToken cancellationToken);
	Task<List<TopProductRow>> GetTopProductsAsync(int limit, DateOnly from, DateOnly to, string? type, CancellationToken cancellationToken);
	Task<List<TopClientRow>> GetTopClientsAsync(int limit, DateOnly from, DateOnly to, CancellationToken cancellationToken);
	Task<List<OrdersByStatusRow>> GetOrdersByStatusAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
	Task<List<StockAlertRow>> GetStockAlertsAsync(int threshold, CancellationToken cancellationToken);
	Task<CatalogSummaryRow> GetCatalogSummaryAsync(CancellationToken cancellationToken);
	Task<RealtimeSummaryRow> GetRealtimeSummaryAsync(int stockThreshold, CancellationToken cancellationToken);
	Task<RealtimeLastOrder?> GetLastOrderAsync(CancellationToken cancellationToken);
}
