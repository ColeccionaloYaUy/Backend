using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Persistence.Carts.Interfaces;
using ColeccionaloYa.Persistence.Carts.ReadModels;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Carts.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class CartRepository : ICartRepository {
	private readonly ICConnection _Connection;

	public CartRepository(ICConnection connection) {
		_Connection = connection;
	}

	public async Task<int?> GetCartIdAsync(int clientId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT id_cart FROM cart WHERE id_client = @clientId ORDER BY id_cart LIMIT 1";
		cmd.AddParameter("clientId", clientId);
		var data = await cmd.ExecuteSelect(rs => new { Id = rs.GetValue<int>("id_cart") }, cancellationToken);
		return data?.Id;
	}

	public async Task<int> EnsureCartAsync(int clientId, CancellationToken cancellationToken) {
		var existing = await GetCartIdAsync(clientId, cancellationToken);
		if (existing.HasValue) {
			return existing.Value;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO cart (id_client, creation_date, updated_date)
            VALUES (@clientId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            RETURNING id_cart";
		cmd.AddParameter("clientId", clientId);
		return await cmd.ExecuteGetValue<int>("id_cart", cancellationToken);
	}

	public async Task<CartData?> GetCartDataAsync(int clientId, CancellationToken cancellationToken) {
		var headerCmd = _Connection.CreateCommand();
		headerCmd.CommandText = @"
            SELECT c.id_cart, c.id_client, c.id_coupon, c.creation_date, c.updated_date,
                   co.name AS coupon_name, co.token AS coupon_token, co.porcentage AS coupon_porcentage
            FROM cart c
            LEFT JOIN coupon co ON co.id_coupon = c.id_coupon AND co.logical_delete = FALSE
            WHERE c.id_client = @clientId
            ORDER BY c.id_cart
            LIMIT 1";
		headerCmd.AddParameter("clientId", clientId);

		var header = await headerCmd.ExecuteSelect(rs => {
			var couponId = rs.GetValue<int?>("id_coupon");
			CartCouponData? coupon = couponId.HasValue && rs.GetValue<string?>("coupon_name") is not null
				? new CartCouponData(couponId.Value, rs.GetValue<string>("coupon_name"), rs.GetValue<string>("coupon_token"), rs.GetValue<decimal>("coupon_porcentage"))
				: null;
			return new CartData(
				rs.GetValue<int>("id_cart"),
				rs.GetValue<int>("id_client"),
				couponId,
				rs.GetValue<DateTime>("creation_date"),
				rs.GetValue<DateTime>("updated_date"),
				new List<CartItemData>(),
				coupon);
		}, cancellationToken);

		if (header is null) {
			return null;
		}

		var itemsCmd = _Connection.CreateCommand();
		itemsCmd.CommandText = @"
            SELECT ci.id_product, p.name, ci.quantity, ci.added_date, p.price,
                   (SELECT pg.url FROM product_gallery pg WHERE pg.id_product = p.id_product ORDER BY pg.""order"" ASC LIMIT 1) AS cover_url,
                   COALESCE((SELECT SUM(s.input) - SUM(s.output) FROM stock s WHERE s.id_product = p.id_product), 0) AS current_stock,
                   COALESCE(d.porcentage, 0) AS discount_porcentage
            FROM cart_item ci
            INNER JOIN product p ON p.id_product = ci.id_product
            LEFT JOIN LATERAL (
                SELECT dd.porcentage
                FROM discount dd
                WHERE dd.id_product = p.id_product
                  AND dd.is_active = TRUE AND dd.logical_delete = FALSE
                  AND (dd.valid_from IS NULL OR dd.valid_from <= NOW())
                  AND (dd.valid_until IS NULL OR dd.valid_until >= NOW())
                ORDER BY dd.porcentage DESC
                LIMIT 1
            ) d ON TRUE
            WHERE ci.id_cart = @cartId
            ORDER BY ci.added_date ASC, ci.id_product ASC";
		itemsCmd.AddParameter("cartId", header.IdCart);

		var items = await itemsCmd.ExecuteSelectList<CartItemData>(rs => {
			var price = rs.GetValue<decimal>("price");
			var discountPorcentage = rs.GetValue<decimal>("discount_porcentage");
			var discountedPrice = Math.Round(price * (1 - discountPorcentage / 100m), 2);
			return new CartItemData(
				rs.GetValue<int>("id_product"),
				rs.GetValue<string>("name"),
				rs.GetValue<string?>("cover_url"),
				price,
				discountedPrice,
				rs.GetValue<int>("quantity"),
				rs.GetValue<DateTime>("added_date"),
				rs.GetValue<int>("current_stock"));
		}, cancellationToken);

		return header with { Items = items };
	}

	public async Task<bool> ItemExistsAsync(int cartId, int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM cart_item WHERE id_cart = @cartId AND id_product = @productId";
		cmd.AddParameter("cartId", cartId);
		cmd.AddParameter("productId", productId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<int?> GetItemQuantityAsync(int cartId, int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT quantity FROM cart_item WHERE id_cart = @cartId AND id_product = @productId";
		cmd.AddParameter("cartId", cartId);
		cmd.AddParameter("productId", productId);
		var data = await cmd.ExecuteSelect(rs => new { Q = rs.GetValue<int>("quantity") }, cancellationToken);
		return data?.Q;
	}

	public async Task InsertItemAsync(int cartId, int productId, int quantity, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO cart_item (id_cart, id_product, quantity, added_date)
            VALUES (@cartId, @productId, @quantity, CURRENT_TIMESTAMP)";
		cmd.AddParameter("cartId", cartId);
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("quantity", quantity);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task SetItemQuantityAsync(int cartId, int productId, int quantity, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "UPDATE cart_item SET quantity = @quantity WHERE id_cart = @cartId AND id_product = @productId";
		cmd.AddParameter("cartId", cartId);
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("quantity", quantity);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task RemoveItemAsync(int cartId, int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM cart_item WHERE id_cart = @cartId AND id_product = @productId";
		cmd.AddParameter("cartId", cartId);
		cmd.AddParameter("productId", productId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task ClearItemsAsync(int cartId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM cart_item WHERE id_cart = @cartId";
		cmd.AddParameter("cartId", cartId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);

		await SetCouponAsync(cartId, null, cancellationToken);
	}

	public async Task SetCouponAsync(int cartId, int? couponId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "UPDATE cart SET id_coupon = @couponId, updated_date = CURRENT_TIMESTAMP WHERE id_cart = @cartId";
		cmd.AddParameter("cartId", cartId);
		cmd.AddParameter("couponId", (object?)couponId ?? DBNull.Value);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task TouchAsync(int cartId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "UPDATE cart SET updated_date = CURRENT_TIMESTAMP WHERE id_cart = @cartId";
		cmd.AddParameter("cartId", cartId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task<decimal> GetNetSubtotalAsync(int clientId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT COALESCE(SUM(
                ci.quantity * ROUND(p.price * (1 - COALESCE(d.porcentage, 0) / 100), 2)
            ), 0) AS net_subtotal
            FROM cart c
            INNER JOIN cart_item ci ON ci.id_cart = c.id_cart
            INNER JOIN product p ON p.id_product = ci.id_product
            LEFT JOIN LATERAL (
                SELECT dd.porcentage
                FROM discount dd
                WHERE dd.id_product = p.id_product
                  AND dd.is_active = TRUE AND dd.logical_delete = FALSE
                  AND (dd.valid_from IS NULL OR dd.valid_from <= NOW())
                  AND (dd.valid_until IS NULL OR dd.valid_until >= NOW())
                ORDER BY dd.porcentage DESC
                LIMIT 1
            ) d ON TRUE
            WHERE c.id_client = @clientId";
		cmd.AddParameter("clientId", clientId);
		return await cmd.ExecuteGetValue<decimal>("net_subtotal", cancellationToken);
	}
}
