using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Persistence.Dashboard.Interfaces;
using ColeccionaloYa.Persistence.Dashboard.ReadModels;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Dashboard.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class DashboardRepository : IDashboardRepository {
	// Orders that represent realized sales.
	private const string ValidSales = "('confirmed','processing','shipped','delivered')";

	private readonly ICConnection _Connection;

	public DashboardRepository(ICConnection connection) {
		_Connection = connection;
	}

	public async Task<SummaryTotals> GetSummaryAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = $@"
            SELECT
                COALESCE((SELECT SUM(o.total) FROM ""order"" o
                          WHERE o.creation_date BETWEEN @from AND @to AND o.status IN {ValidSales}), 0) AS total_sales,
                COALESCE((SELECT COUNT(*) FROM ""order"" o
                          WHERE o.creation_date BETWEEN @from AND @to AND o.status <> 'cancelled'), 0) AS orders_count,
                COALESCE((SELECT COUNT(*) FROM client c
                          INNER JOIN roles r ON r.id = c.role_id
                          WHERE c.creation_date::date BETWEEN @from AND @to AND r.name = 'User' AND c.logical_delete = FALSE), 0) AS new_clients,
                COALESCE((SELECT SUM(ol.quantity) FROM order_line ol
                          INNER JOIN ""order"" o ON o.id_order = ol.id_order
                          WHERE o.creation_date BETWEEN @from AND @to AND o.status IN {ValidSales}), 0) AS total_units_sold,
                COALESCE((SELECT COUNT(DISTINCT o.id_client) FROM ""order"" o
                          WHERE o.creation_date BETWEEN @from AND @to AND o.status IN {ValidSales}), 0) AS active_clients";
		cmd.AddParameter("from", from);
		cmd.AddParameter("to", to);
		var result = await cmd.ExecuteSelect(rs => new SummaryTotals(
			rs.GetValue<decimal>("total_sales"),
			(int)rs.GetValue<long>("orders_count"),
			(int)rs.GetValue<long>("new_clients"),
			(int)rs.GetValue<long>("total_units_sold"),
			(int)rs.GetValue<long>("active_clients")), cancellationToken);
		return result ?? new SummaryTotals(0, 0, 0, 0, 0);
	}

	public async Task<List<SalesBucket>> GetSalesSeriesAsync(string period, DateOnly from, DateOnly to, string statusFilter, CancellationToken cancellationToken) {
		var trunc = period switch {
			"week" => "week",
			"month" => "month",
			_ => "day",
		};

		var statusCondition = statusFilter switch {
			"all" => "TRUE",
			"delivered" => "o.status = 'delivered'",
			_ => "o.status <> 'cancelled'", // valid
		};

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = $@"
            SELECT date_trunc('{trunc}', o.creation_date)::date AS bucket,
                   COALESCE(SUM(o.total), 0) AS total,
                   COALESCE(SUM(o.subtotal), 0) AS subtotal,
                   COALESCE(SUM(o.taxes), 0) AS taxes,
                   COUNT(*) AS orders,
                   COALESCE(SUM((SELECT SUM(ol.quantity) FROM order_line ol WHERE ol.id_order = o.id_order)), 0) AS units
            FROM ""order"" o
            WHERE o.creation_date BETWEEN @from AND @to AND {statusCondition}
            GROUP BY bucket
            ORDER BY bucket ASC";
		cmd.AddParameter("from", from);
		cmd.AddParameter("to", to);
		return await cmd.ExecuteSelectList<SalesBucket>(rs => new SalesBucket(
			rs.GetValue<DateOnly>("bucket"),
			rs.GetValue<decimal>("total"),
			rs.GetValue<decimal>("subtotal"),
			rs.GetValue<decimal>("taxes"),
			(int)rs.GetValue<long>("orders"),
			(int)rs.GetValue<long>("units")), cancellationToken);
	}

	public async Task<List<TopProductRow>> GetTopProductsAsync(int limit, DateOnly from, DateOnly to, string? type, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = $@"
            SELECT p.id_product, p.name, p.type::text AS type, p.price,
                   COALESCE(SUM(ol.quantity), 0) AS quantity_sold,
                   COALESCE(SUM(ol.quantity * ol.price), 0) AS revenue,
                   COUNT(DISTINCT o.id_order) AS orders_count,
                   COALESCE((SELECT SUM(s.input) - SUM(s.output) FROM stock s WHERE s.id_product = p.id_product), 0) AS current_stock,
                   (SELECT pg.url FROM product_gallery pg WHERE pg.id_product = p.id_product ORDER BY pg.""order"" ASC LIMIT 1) AS cover_url
            FROM order_line ol
            INNER JOIN ""order"" o ON o.id_order = ol.id_order
            INNER JOIN product p ON p.id_product = ol.id_product
            WHERE o.creation_date BETWEEN @from AND @to AND o.status IN {ValidSales}
              AND (@type IS NULL OR p.type = @type::product_type)
            GROUP BY p.id_product, p.name, p.type, p.price
            ORDER BY quantity_sold DESC, revenue DESC
            LIMIT @limit";
		cmd.AddParameter("from", from);
		cmd.AddParameter("to", to);
		cmd.AddParameter("type", (object?)type ?? DBNull.Value);
		cmd.AddParameter("limit", limit);
		return await cmd.ExecuteSelectList<TopProductRow>(rs => new TopProductRow(
			rs.GetValue<int>("id_product"),
			rs.GetValue<string>("name"),
			rs.GetValue<string>("type"),
			rs.GetValue<string?>("cover_url"),
			rs.GetValue<decimal>("price"),
			(int)rs.GetValue<long>("quantity_sold"),
			rs.GetValue<decimal>("revenue"),
			(int)rs.GetValue<long>("orders_count"),
			rs.GetValue<int>("current_stock")), cancellationToken);
	}

	public async Task<List<TopClientRow>> GetTopClientsAsync(int limit, DateOnly from, DateOnly to, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = $@"
            SELECT cl.id_client, cl.name, cl.lastname, cl.email,
                   COALESCE(SUM(o.total), 0) AS total_spent,
                   COUNT(o.id_order) AS orders_count,
                   COALESCE(SUM((SELECT SUM(ol.quantity) FROM order_line ol WHERE ol.id_order = o.id_order)), 0) AS units_purchased,
                   MAX(o.creation_date) AS last_order_date,
                   cl.creation_date::date AS client_since
            FROM ""order"" o
            INNER JOIN client cl ON cl.id_client = o.id_client
            WHERE o.creation_date BETWEEN @from AND @to AND o.status IN {ValidSales}
            GROUP BY cl.id_client, cl.name, cl.lastname, cl.email, cl.creation_date
            ORDER BY total_spent DESC
            LIMIT @limit";
		cmd.AddParameter("from", from);
		cmd.AddParameter("to", to);
		cmd.AddParameter("limit", limit);
		return await cmd.ExecuteSelectList<TopClientRow>(rs => new TopClientRow(
			rs.GetValue<int>("id_client"),
			rs.GetValue<string>("name"),
			rs.GetValue<string>("lastname"),
			rs.GetValue<string>("email"),
			rs.GetValue<decimal>("total_spent"),
			(int)rs.GetValue<long>("orders_count"),
			(int)rs.GetValue<long>("units_purchased"),
			rs.GetValue<DateOnly?>("last_order_date"),
			rs.GetValue<DateOnly>("client_since")), cancellationToken);
	}

	public async Task<List<OrdersByStatusRow>> GetOrdersByStatusAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT o.status::text AS status, COUNT(*) AS count, COALESCE(SUM(o.total), 0) AS total_value
            FROM ""order"" o
            WHERE o.creation_date BETWEEN @from AND @to
            GROUP BY o.status
            ORDER BY count DESC";
		cmd.AddParameter("from", from);
		cmd.AddParameter("to", to);
		return await cmd.ExecuteSelectList<OrdersByStatusRow>(rs => new OrdersByStatusRow(
			rs.GetValue<string>("status"),
			(int)rs.GetValue<long>("count"),
			rs.GetValue<decimal>("total_value")), cancellationToken);
	}

	public async Task<List<StockAlertRow>> GetStockAlertsAsync(int threshold, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT p.id_product, p.name, p.type::text AS type,
                   COALESCE((SELECT SUM(s.input) - SUM(s.output) FROM stock s WHERE s.id_product = p.id_product), 0) AS current_stock,
                   COALESCE((SELECT SUM(ol.quantity)::numeric / 30 FROM order_line ol
                             INNER JOIN ""order"" o ON o.id_order = ol.id_order
                             WHERE ol.id_product = p.id_product AND o.creation_date >= (CURRENT_DATE - INTERVAL '30 days')), 0) AS avg_daily_sales,
                   (SELECT pg.url FROM product_gallery pg WHERE pg.id_product = p.id_product ORDER BY pg.""order"" ASC LIMIT 1) AS cover_url
            FROM product p
            WHERE p.logical_delete = FALSE
              AND COALESCE((SELECT SUM(s.input) - SUM(s.output) FROM stock s WHERE s.id_product = p.id_product), 0) <= @threshold
            ORDER BY current_stock ASC";
		cmd.AddParameter("threshold", threshold);
		return await cmd.ExecuteSelectList<StockAlertRow>(rs => new StockAlertRow(
			rs.GetValue<int>("id_product"),
			rs.GetValue<string>("name"),
			rs.GetValue<string>("type"),
			rs.GetValue<string?>("cover_url"),
			rs.GetValue<int>("current_stock"),
			rs.GetValue<decimal>("avg_daily_sales")), cancellationToken);
	}

	public async Task<CatalogSummaryRow> GetCatalogSummaryAsync(CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT
                (SELECT COUNT(*) FROM product WHERE logical_delete = FALSE) AS total_products,
                (SELECT COUNT(*) FROM product WHERE logical_delete = FALSE AND type = 'book') AS total_books,
                (SELECT COUNT(*) FROM product WHERE logical_delete = FALSE AND type = 'pack') AS total_packs,
                (SELECT COUNT(*) FROM product WHERE logical_delete = FALSE AND type = 'object') AS total_objects,
                (SELECT COUNT(*) FROM book b INNER JOIN product p ON p.id_product = b.id_product WHERE p.logical_delete = FALSE AND b.type = 'comic') AS total_comics,
                (SELECT COUNT(*) FROM book b INNER JOIN product p ON p.id_product = b.id_product WHERE p.logical_delete = FALSE AND b.type = 'manga') AS total_mangas,
                (SELECT COUNT(*) FROM product WHERE logical_delete = FALSE AND is_featured = TRUE) AS featured_products,
                (SELECT COUNT(DISTINCT d.id_product) FROM discount d WHERE d.is_active = TRUE AND d.logical_delete = FALSE) AS products_with_discount,
                (SELECT COUNT(*) FROM product p WHERE p.logical_delete = FALSE AND NOT EXISTS (SELECT 1 FROM product_gallery pg WHERE pg.id_product = p.id_product)) AS products_without_image,
                (SELECT COUNT(*) FROM author) AS total_authors,
                (SELECT COUNT(*) FROM genre) AS total_genres,
                (SELECT COUNT(*) FROM tag WHERE is_franchise = TRUE) AS total_franchises,
                (SELECT COUNT(*) FROM tag) AS total_tags,
                COALESCE((SELECT AVG(price) FROM product WHERE logical_delete = FALSE), 0) AS avg_price";
		var result = await cmd.ExecuteSelect(rs => new CatalogSummaryRow(
			(int)rs.GetValue<long>("total_products"),
			(int)rs.GetValue<long>("total_books"),
			(int)rs.GetValue<long>("total_packs"),
			(int)rs.GetValue<long>("total_objects"),
			(int)rs.GetValue<long>("total_comics"),
			(int)rs.GetValue<long>("total_mangas"),
			(int)rs.GetValue<long>("featured_products"),
			(int)rs.GetValue<long>("products_with_discount"),
			(int)rs.GetValue<long>("products_without_image"),
			(int)rs.GetValue<long>("total_authors"),
			(int)rs.GetValue<long>("total_genres"),
			(int)rs.GetValue<long>("total_franchises"),
			(int)rs.GetValue<long>("total_tags"),
			rs.GetValue<decimal>("avg_price")), cancellationToken);
		return result ?? new CatalogSummaryRow(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
	}

	public async Task<RealtimeSummaryRow> GetRealtimeSummaryAsync(int stockThreshold, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = $@"
            SELECT
                (SELECT COUNT(*) FROM ""order"" WHERE creation_date >= (CURRENT_DATE - INTERVAL '1 day')) AS orders_last_24h,
                COALESCE((SELECT SUM(total) FROM ""order"" WHERE creation_date >= (CURRENT_DATE - INTERVAL '1 day') AND status IN {ValidSales}), 0) AS revenue_last_24h,
                (SELECT COUNT(*) FROM client WHERE creation_date >= (NOW() - INTERVAL '1 day')) AS new_clients_last_24h,
                (SELECT COUNT(*) FROM cart WHERE updated_date >= (NOW() - INTERVAL '1 hour')) AS active_carts_now,
                (SELECT COUNT(*) FROM ""order"" WHERE status = 'pending') AS pending_orders,
                (SELECT COUNT(*) FROM ""order"" WHERE status = 'processing') AS processing_orders,
                (SELECT COUNT(*) FROM ""order"" WHERE status = 'shipped') AS shipped_orders,
                (SELECT COUNT(*) FROM payment WHERE status = 'pending') AS pending_payments,
                (SELECT COUNT(*) FROM product p WHERE p.logical_delete = FALSE
                    AND COALESCE((SELECT SUM(s.input) - SUM(s.output) FROM stock s WHERE s.id_product = p.id_product), 0) <= @threshold) AS stock_alerts";
		cmd.AddParameter("threshold", stockThreshold);
		var result = await cmd.ExecuteSelect(rs => new RealtimeSummaryRow(
			(int)rs.GetValue<long>("orders_last_24h"),
			rs.GetValue<decimal>("revenue_last_24h"),
			(int)rs.GetValue<long>("new_clients_last_24h"),
			(int)rs.GetValue<long>("active_carts_now"),
			(int)rs.GetValue<long>("pending_orders"),
			(int)rs.GetValue<long>("processing_orders"),
			(int)rs.GetValue<long>("shipped_orders"),
			(int)rs.GetValue<long>("pending_payments"),
			(int)rs.GetValue<long>("stock_alerts")), cancellationToken);
		return result ?? new RealtimeSummaryRow(0, 0, 0, 0, 0, 0, 0, 0, 0);
	}

	public async Task<RealtimeLastOrder?> GetLastOrderAsync(CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT o.id_order, cl.name || ' ' || cl.lastname AS client_name, COALESCE(o.total, 0) AS total, o.creation_date
            FROM ""order"" o
            INNER JOIN client cl ON cl.id_client = o.id_client
            ORDER BY o.id_order DESC
            LIMIT 1";
		return await cmd.ExecuteSelect(rs => new RealtimeLastOrder(
			rs.GetValue<int>("id_order"),
			rs.GetValue<string>("client_name"),
			rs.GetValue<decimal>("total"),
			rs.GetValue<DateOnly>("creation_date")), cancellationToken);
	}
}
