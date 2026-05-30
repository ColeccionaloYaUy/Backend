using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Orders;
using ColeccionaloYa.Persistence.Orders.Interfaces;
using ColeccionaloYa.Persistence.Orders.ReadModels;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Orders.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class OrderRepository : IOrderRepository {
	private readonly ICConnection _Connection;

	public OrderRepository(ICConnection connection) {
		_Connection = connection;
	}

	public async Task<PagedData<OrderListItem>> GetPagedAsync(int page, int pageSize, string? status, DateOnly? dateFrom, DateOnly? dateTo, int? clientId, CancellationToken cancellationToken) {
		var sql = @"
            SELECT COUNT(*) OVER() AS total_count,
                   o.id_order, o.creation_date, o.id_client, cl.name || ' ' || cl.lastname AS client_name,
                   cl.email AS client_email, o.id_coupon, o.status::text AS status, o.tracking,
                   COALESCE(o.subtotal, 0) AS subtotal, o.taxes, COALESCE(o.total, 0) AS total,
                   COALESCE((SELECT SUM(ol.quantity) FROM order_line ol WHERE ol.id_order = o.id_order), 0) AS items_count
            FROM ""order"" o
            INNER JOIN client cl ON cl.id_client = o.id_client
            WHERE (@status IS NULL OR o.status = @status::type_order_status)
              AND (@dateFrom IS NULL OR o.creation_date >= @dateFrom)
              AND (@dateTo IS NULL OR o.creation_date <= @dateTo)
              AND (@clientId IS NULL OR o.id_client = @clientId)
            ORDER BY o.id_order DESC";

		return await PaginationHelper.FetchPagedAsync<OrderListItem>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("status", (object?)status ?? DBNull.Value);
				cmd.AddParameter("dateFrom", (object?)dateFrom ?? DBNull.Value);
				cmd.AddParameter("dateTo", (object?)dateTo ?? DBNull.Value);
				cmd.AddParameter("clientId", (object?)clientId ?? DBNull.Value);
			},
			rs => new OrderListItem(
				rs.GetValue<int>("id_order"),
				rs.GetValue<DateOnly>("creation_date"),
				rs.GetValue<int>("id_client"),
				rs.GetValue<string>("client_name"),
				rs.GetValue<string>("client_email"),
				rs.GetValue<int?>("id_coupon"),
				rs.GetValue<string>("status"),
				rs.GetValue<string>("tracking"),
				rs.GetValue<decimal>("subtotal"),
				rs.GetValue<decimal>("taxes"),
				rs.GetValue<decimal>("total"),
				(int)rs.GetValue<long>("items_count")),
			page,
			pageSize,
			cancellationToken);
	}

	public async Task<int?> GetOwnerClientIdAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"SELECT id_client FROM ""order"" WHERE id_order = @id";
		cmd.AddParameter("id", orderId);
		var data = await cmd.ExecuteSelect(rs => new { Id = rs.GetValue<int>("id_client") }, cancellationToken);
		return data?.Id;
	}

	private static OrderAddressSnapshotData MapSnapshot(ICDataReader rs, string prefix) {
		return new OrderAddressSnapshotData(
			rs.GetValue<int>($"{prefix}_id_snapshot"),
			rs.GetValue<string>($"{prefix}_state"),
			rs.GetValue<string>($"{prefix}_type"),
			rs.GetValue<string?>($"{prefix}_observations"),
			rs.GetValue<string?>($"{prefix}_apartment"),
			rs.GetValue<string?>($"{prefix}_corner"),
			rs.GetValue<string?>($"{prefix}_door_number"),
			rs.GetValue<string?>($"{prefix}_floor"),
			rs.GetValue<string>($"{prefix}_name"),
			rs.GetValue<string>($"{prefix}_neighborhood"),
			rs.GetValue<string>($"{prefix}_street"));
	}

	public async Task<OrderDetailData?> GetDetailAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT o.id_order, o.creation_date, o.id_client, o.id_coupon, o.taxes, o.status::text AS status,
                   o.tracking, o.observations, o.cancel_reason, o.return_reason,
                   COALESCE(o.subtotal, 0) AS subtotal, COALESCE(o.total, 0) AS total,
                   cl.name AS client_name, cl.lastname AS client_lastname, cl.email AS client_email, cl.phone AS client_phone,
                   co.name AS coupon_name, co.token AS coupon_token, co.porcentage AS coupon_porcentage,
                   ds.id_snapshot AS d_id_snapshot, ds.state AS d_state, ds.type::text AS d_type, ds.observations AS d_observations,
                   ds.apartment AS d_apartment, ds.corner AS d_corner, ds.door_number AS d_door_number, ds.floor AS d_floor,
                   ds.name AS d_name, ds.neighborhood AS d_neighborhood, ds.street AS d_street,
                   bs.id_snapshot AS b_id_snapshot, bs.state AS b_state, bs.type::text AS b_type, bs.observations AS b_observations,
                   bs.apartment AS b_apartment, bs.corner AS b_corner, bs.door_number AS b_door_number, bs.floor AS b_floor,
                   bs.name AS b_name, bs.neighborhood AS b_neighborhood, bs.street AS b_street
            FROM ""order"" o
            INNER JOIN client cl ON cl.id_client = o.id_client
            LEFT JOIN coupon co ON co.id_coupon = o.id_coupon
            LEFT JOIN order_address_snapshot ds ON ds.id_snapshot = o.id_delivery_snapshot
            LEFT JOIN order_address_snapshot bs ON bs.id_snapshot = o.id_order_snapshot
            WHERE o.id_order = @id";
		cmd.AddParameter("id", orderId);

		var header = await cmd.ExecuteSelect(rs => {
			var order = new OrderHeaderData(
				rs.GetValue<int>("id_order"),
				rs.GetValue<DateOnly>("creation_date"),
				rs.GetValue<int>("id_client"),
				rs.GetValue<int?>("id_coupon"),
				rs.GetValue<decimal>("taxes"),
				rs.GetValue<string>("status"),
				rs.GetValue<string>("tracking"),
				rs.GetValue<string>("observations"),
				rs.GetValue<string?>("cancel_reason"),
				rs.GetValue<string?>("return_reason"),
				rs.GetValue<decimal>("subtotal"),
				rs.GetValue<decimal>("total"));

			var client = new OrderClientInfo(
				rs.GetValue<int>("id_client"),
				rs.GetValue<string>("client_name"),
				rs.GetValue<string>("client_lastname"),
				rs.GetValue<string>("client_email"),
				rs.GetValue<string?>("client_phone") ?? string.Empty);

			var deliveryId = rs.GetValue<int?>("d_id_snapshot");
			var delivery = deliveryId.HasValue ? MapSnapshot(rs, "d") : null;
			var billingId = rs.GetValue<int?>("b_id_snapshot");
			var billing = billingId.HasValue ? MapSnapshot(rs, "b") : null;

			var couponId = rs.GetValue<int?>("id_coupon");
			OrderCouponInfo? coupon = couponId.HasValue && rs.GetValue<string?>("coupon_name") is not null
				? new OrderCouponInfo(couponId.Value, rs.GetValue<string>("coupon_name"), rs.GetValue<string>("coupon_token"), rs.GetValue<decimal>("coupon_porcentage"))
				: null;

			return new OrderDetailData(order, new List<OrderLineDetail>(), client, delivery, billing, coupon);
		}, cancellationToken);

		if (header is null) {
			return null;
		}

		var linesCmd = _Connection.CreateCommand();
		linesCmd.CommandText = @"
            SELECT ol.id_product, p.name AS product_name, ol.quantity, ol.price,
                   (ol.price * ol.quantity) AS line_total,
                   (SELECT pg.url FROM product_gallery pg WHERE pg.id_product = p.id_product ORDER BY pg.""order"" ASC LIMIT 1) AS cover_url
            FROM order_line ol
            INNER JOIN product p ON p.id_product = ol.id_product
            WHERE ol.id_order = @id
            ORDER BY p.name";
		linesCmd.AddParameter("id", orderId);
		var lines = await linesCmd.ExecuteSelectList<OrderLineDetail>(rs => new OrderLineDetail(
			rs.GetValue<int>("id_product"),
			rs.GetValue<string>("product_name"),
			rs.GetValue<string?>("cover_url"),
			rs.GetValue<int>("quantity"),
			rs.GetValue<decimal>("price"),
			rs.GetValue<decimal>("line_total")), cancellationToken);

		return header with { Lines = lines };
	}

	public async Task<OrderTrackingData?> GetTrackingAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"SELECT o.tracking, o.status::text AS status FROM ""order"" o WHERE o.id_order = @id";
		cmd.AddParameter("id", orderId);
		var header = await cmd.ExecuteSelect(rs => new {
			Tracking = rs.GetValue<string>("tracking"),
			Status = rs.GetValue<string>("status"),
		}, cancellationToken);

		if (header is null) {
			return null;
		}

		var histCmd = _Connection.CreateCommand();
		histCmd.CommandText = @"
            SELECT h.id, h.status::text AS status, h.reason, h.changed_by, h.date
            FROM order_status_history h
            WHERE h.id_order = @id
            ORDER BY h.date ASC, h.id ASC";
		histCmd.AddParameter("id", orderId);
		var history = await histCmd.ExecuteSelectList<OrderStatusHistoryEntry>(rs => new OrderStatusHistoryEntry(
			rs.GetValue<int>("id"),
			rs.GetValue<string>("status"),
			rs.GetValue<string?>("reason"),
			rs.GetValue<int?>("changed_by"),
			rs.GetValue<DateTime>("date")), cancellationToken);

		return new OrderTrackingData(header.Tracking, header.Status, history);
	}

	public async Task<Order?> GetEntityAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT o.id_order, o.creation_date, o.id_client, o.id_coupon, o.taxes, o.status::text AS status,
                   o.tracking, o.observations, o.cancel_reason, o.return_reason,
                   COALESCE(o.subtotal, 0) AS subtotal, COALESCE(o.total, 0) AS total,
                   o.id_delivery_snapshot, o.id_order_snapshot
            FROM ""order"" o
            WHERE o.id_order = @id";
		cmd.AddParameter("id", orderId);
		return await cmd.ExecuteSelect(rs => new Order {
			Id = rs.GetValue<int>("id_order"),
			CreationDate = rs.GetValue<DateOnly>("creation_date"),
			ClientId = rs.GetValue<int>("id_client"),
			CouponId = rs.GetValue<int?>("id_coupon"),
			Taxes = rs.GetValue<decimal>("taxes"),
			Status = Enum.Parse<OrderStatus>(rs.GetValue<string>("status"), ignoreCase: true),
			Tracking = rs.GetValue<string>("tracking"),
			Observations = rs.GetValue<string>("observations"),
			CancelReason = rs.GetValue<string?>("cancel_reason"),
			ReturnReason = rs.GetValue<string?>("return_reason"),
			Subtotal = rs.GetValue<decimal>("subtotal"),
			Total = rs.GetValue<decimal>("total"),
			DeliverySnapshotId = rs.GetValue<int?>("id_delivery_snapshot"),
			OrderSnapshotId = rs.GetValue<int?>("id_order_snapshot"),
		}, cancellationToken);
	}

	public async Task<bool> AddressBelongsToClientAsync(int addressId, int clientId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1 FROM address
            WHERE id_address = @addressId AND id_client = @clientId AND logical_delete = FALSE";
		cmd.AddParameter("addressId", addressId);
		cmd.AddParameter("clientId", clientId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<int> CreateAddressSnapshotAsync(int addressId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO order_address_snapshot
                (id_address_original, state, type, observations, apartment, corner, door_number, floor, name, neighborhood, street, creation_date)
            SELECT a.id_address, a.state, a.type, a.observations, a.apartment, a.corner, a.door_number, a.floor, a.name, a.neighborhood, a.street, CURRENT_TIMESTAMP
            FROM address a
            WHERE a.id_address = @addressId
            RETURNING id_snapshot";
		cmd.AddParameter("addressId", addressId);
		return await cmd.ExecuteGetValue<int>("id_snapshot", cancellationToken);
	}

	public async Task CreateOrderAsync(Order order, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO ""order""
                (creation_date, id_client, id_coupon, taxes, status, tracking, observations, cancel_reason, return_reason, subtotal, total, id_delivery_snapshot, id_order_snapshot)
            VALUES
                (@creationDate, @clientId, @couponId, @taxes, @status::type_order_status, @tracking, @observations, @cancelReason, @returnReason, @subtotal, @total, @deliverySnapshot, @orderSnapshot)
            RETURNING id_order";
		cmd.AddParameter("creationDate", order.CreationDate);
		cmd.AddParameter("clientId", order.ClientId);
		cmd.AddParameter("couponId", (object?)order.CouponId ?? DBNull.Value);
		cmd.AddParameter("taxes", order.Taxes);
		cmd.AddParameter("status", order.Status.ToString().ToLower());
		cmd.AddParameter("tracking", order.Tracking);
		cmd.AddParameter("observations", order.Observations);
		cmd.AddParameter("cancelReason", (object?)order.CancelReason ?? DBNull.Value);
		cmd.AddParameter("returnReason", (object?)order.ReturnReason ?? DBNull.Value);
		cmd.AddParameter("subtotal", order.Subtotal);
		cmd.AddParameter("total", order.Total);
		cmd.AddParameter("deliverySnapshot", (object?)order.DeliverySnapshotId ?? DBNull.Value);
		cmd.AddParameter("orderSnapshot", (object?)order.OrderSnapshotId ?? DBNull.Value);

		var newId = await cmd.ExecuteGetValue<int>("id_order", cancellationToken);
		order.AssignId(newId);
	}

	public async Task CreateOrderLinesAsync(int orderId, IReadOnlyCollection<OrderLine> lines, CancellationToken cancellationToken) {
		foreach (var line in lines) {
			var cmd = _Connection.CreateCommand();
			cmd.CommandText = @"
                INSERT INTO order_line (id_order, id_product, quantity, price)
                VALUES (@orderId, @productId, @quantity, @price)";
			cmd.AddParameter("orderId", orderId);
			cmd.AddParameter("productId", line.ProductId);
			cmd.AddParameter("quantity", line.Quantity);
			cmd.AddParameter("price", line.Price);
			await cmd.ExecuteCommandNonQuery(cancellationToken);
		}
	}

	public async Task AddStatusHistoryAsync(int orderId, OrderStatus status, string? reason, int? changedBy, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO order_status_history (id_order, status, reason, changed_by, date)
            VALUES (@orderId, @status::type_order_status, @reason, @changedBy, CURRENT_TIMESTAMP)";
		cmd.AddParameter("orderId", orderId);
		cmd.AddParameter("status", status.ToString().ToLower());
		cmd.AddParameter("reason", (object?)reason ?? DBNull.Value);
		cmd.AddParameter("changedBy", (object?)changedBy ?? DBNull.Value);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task UpdateStatusAsync(Order order, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE ""order""
            SET status = @status::type_order_status,
                cancel_reason = @cancelReason,
                return_reason = @returnReason
            WHERE id_order = @id";
		cmd.AddParameter("id", order.Id);
		cmd.AddParameter("status", order.Status.ToString().ToLower());
		cmd.AddParameter("cancelReason", (object?)order.CancelReason ?? DBNull.Value);
		cmd.AddParameter("returnReason", (object?)order.ReturnReason ?? DBNull.Value);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task UpdateTrackingAsync(int orderId, string tracking, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"UPDATE ""order"" SET tracking = @tracking WHERE id_order = @id";
		cmd.AddParameter("id", orderId);
		cmd.AddParameter("tracking", tracking);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task<List<(int ProductId, int Quantity)>> GetLineQuantitiesAsync(int orderId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT id_product, quantity FROM order_line WHERE id_order = @id";
		cmd.AddParameter("id", orderId);
		var rows = await cmd.ExecuteSelectList(rs => new {
			ProductId = rs.GetValue<int>("id_product"),
			Quantity = rs.GetValue<int>("quantity"),
		}, cancellationToken);
		return rows.Select(r => (r.ProductId, r.Quantity)).ToList();
	}
}
