using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Stock;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Persistence.Stock.Interfaces;
using ColeccionaloYa.Persistence.Stock.ReadModels;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Stock.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class StockRepository : IStockRepository {
	private readonly ICConnection _Connection;

	public StockRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static StockMovement MapMovement(ICDataReader rs) {
		return new StockMovement {
			Id = rs.GetValue<int>("id_stock"),
			ProductId = rs.GetValue<int>("id_product"),
			Date = rs.GetValue<DateTime>("date"),
			Input = rs.GetValue<int>("input"),
			Output = rs.GetValue<int>("output"),
			OrderId = rs.GetValue<int?>("id_order"),
			Type = Enum.Parse<StockMovementType>(rs.GetValue<string>("type"), ignoreCase: true),
			Reason = rs.GetValue<string?>("reason"),
		};
	}

	public async Task<PagedData<StockMovementWithProduct>> GetPagedAsync(int page, int pageSize, int? productId, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken) {
		var sql = @"
            SELECT COUNT(*) OVER() AS total_count,
                   s.id_stock, s.id_product, s.date, s.input, s.output, s.id_order,
                   s.type::text AS type, s.reason, p.name AS product_name
            FROM stock s
            INNER JOIN product p ON p.id_product = s.id_product
            WHERE (@productId IS NULL OR s.id_product = @productId)
              AND (@dateFrom IS NULL OR s.date >= @dateFrom)
              AND (@dateTo IS NULL OR s.date < (@dateTo::date + INTERVAL '1 day'))
            ORDER BY s.date DESC, s.id_stock DESC";

		return await PaginationHelper.FetchPagedAsync<StockMovementWithProduct>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("productId", (object?)productId ?? DBNull.Value);
				cmd.AddParameter("dateFrom", (object?)dateFrom ?? DBNull.Value);
				cmd.AddParameter("dateTo", (object?)dateTo ?? DBNull.Value);
			},
			rs => new StockMovementWithProduct(MapMovement(rs), rs.GetValue<string>("product_name")),
			page,
			pageSize,
			cancellationToken);
	}

	public async Task<StockSummary> GetSummaryAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT COALESCE(SUM(s.input), 0) AS total_input,
                   COALESCE(SUM(s.output), 0) AS total_output,
                   COALESCE(SUM(s.input) - SUM(s.output), 0) AS current_stock,
                   MAX(s.date) AS last_movement_date
            FROM stock s
            WHERE s.id_product = @id";
		cmd.AddParameter("id", productId);
		var summary = await cmd.ExecuteSelect(rs => new StockSummary(
			productId,
			rs.GetValue<int>("current_stock"),
			rs.GetValue<int>("total_input"),
			rs.GetValue<int>("total_output"),
			rs.GetValue<DateTime?>("last_movement_date")), cancellationToken);
		return summary ?? new StockSummary(productId, 0, 0, 0, null);
	}

	public async Task<int> GetCurrentStockAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT COALESCE(SUM(s.input) - SUM(s.output), 0) AS current_stock
            FROM stock s
            WHERE s.id_product = @id";
		cmd.AddParameter("id", productId);
		return await cmd.ExecuteGetValue<int>("current_stock", cancellationToken);
	}

	public async Task<PagedData<StockMovementHistoryItem>> GetMovementsAsync(int productId, int page, int pageSize, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken) {
		var sql = @"
            SELECT COUNT(*) OVER() AS total_count,
                   s.id_stock, s.id_product, s.date, s.input, s.output, s.id_order,
                   s.type::text AS type, s.reason,
                   SUM(s.input - s.output) OVER (ORDER BY s.date ASC, s.id_stock ASC) AS running_balance
            FROM stock s
            WHERE s.id_product = @productId
              AND (@dateFrom IS NULL OR s.date >= @dateFrom)
              AND (@dateTo IS NULL OR s.date < (@dateTo::date + INTERVAL '1 day'))
            ORDER BY s.date DESC, s.id_stock DESC";

		return await PaginationHelper.FetchPagedAsync<StockMovementHistoryItem>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("productId", productId);
				cmd.AddParameter("dateFrom", (object?)dateFrom ?? DBNull.Value);
				cmd.AddParameter("dateTo", (object?)dateTo ?? DBNull.Value);
			},
			rs => new StockMovementHistoryItem(MapMovement(rs), rs.GetValue<int>("running_balance")),
			page,
			pageSize,
			cancellationToken);
	}

	public async Task<bool> OrderExistsAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"SELECT 1 FROM ""order"" WHERE id_order = @id";
		cmd.AddParameter("id", orderId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(StockMovement movement, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO stock (id_product, date, input, output, id_order, type, reason)
            VALUES (@productId, @date, @input, @output, @orderId, @type::stock_movement_type, @reason)
            RETURNING id_stock";
		cmd.AddParameter("productId", movement.ProductId);
		cmd.AddParameter("date", movement.Date);
		cmd.AddParameter("input", movement.Input);
		cmd.AddParameter("output", movement.Output);
		cmd.AddParameter("orderId", (object?)movement.OrderId ?? DBNull.Value);
		cmd.AddParameter("type", movement.Type.ToString().ToLower());
		cmd.AddParameter("reason", (object?)movement.Reason ?? DBNull.Value);

		var newId = await cmd.ExecuteGetValue<int>("id_stock", cancellationToken);
		movement.AssignId(newId);
	}
}
