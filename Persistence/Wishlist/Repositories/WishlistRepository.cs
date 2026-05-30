using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Persistence.Products.ReadModels;
using ColeccionaloYa.Persistence.Wishlist.Interfaces;
using ColeccionaloYa.Persistence.Wishlist.ReadModels;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Wishlist.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class WishlistRepository : IWishlistRepository {
	private readonly ICConnection _Connection;

	public WishlistRepository(ICConnection connection) {
		_Connection = connection;
	}

	public async Task<List<WishlistProductItem>> GetByClientAsync(int clientId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT p.id_product, p.name, p.short_description, p.price, p.type::text AS type,
                   (SELECT pg.url FROM product_gallery pg WHERE pg.id_product = p.id_product ORDER BY pg.""order"" ASC LIMIT 1) AS cover_url,
                   COALESCE((SELECT SUM(s.input) - SUM(s.output) FROM stock s WHERE s.id_product = p.id_product), 0) AS current_stock,
                   d.id_discount AS discount_id,
                   d.porcentage AS discount_porcentage,
                   d.valid_from AS discount_valid_from,
                   d.valid_until AS discount_valid_until,
                   (p.price * (1 - d.porcentage / 100)) AS discount_final_price
            FROM wishlist w
            INNER JOIN product p ON p.id_product = w.id_product AND p.logical_delete = FALSE
            LEFT JOIN LATERAL (
                SELECT dd.id_discount, dd.porcentage, dd.valid_from, dd.valid_until
                FROM discount dd
                WHERE dd.id_product = p.id_product
                  AND dd.is_active = TRUE AND dd.logical_delete = FALSE
                  AND (dd.valid_from IS NULL OR dd.valid_from <= NOW())
                  AND (dd.valid_until IS NULL OR dd.valid_until >= NOW())
                ORDER BY dd.porcentage DESC
                LIMIT 1
            ) d ON TRUE
            WHERE w.id_client = @clientId
            ORDER BY p.name";
		cmd.AddParameter("clientId", clientId);

		return await cmd.ExecuteSelectList<WishlistProductItem>(rs => {
			var discountId = rs.GetValue<int?>("discount_id");
			ActiveDiscountInfo? discount = discountId.HasValue
				? new ActiveDiscountInfo(
					discountId.Value,
					rs.GetValue<decimal>("discount_porcentage"),
					rs.GetValue<DateTime?>("discount_valid_from"),
					rs.GetValue<DateTime?>("discount_valid_until"),
					rs.GetValue<decimal>("discount_final_price"))
				: null;

			return new WishlistProductItem(
				rs.GetValue<int>("id_product"),
				rs.GetValue<string>("name"),
				rs.GetValue<string>("short_description"),
				rs.GetValue<decimal>("price"),
				rs.GetValue<string>("type"),
				rs.GetValue<string?>("cover_url"),
				rs.GetValue<int>("current_stock"),
				discount);
		}, cancellationToken);
	}

	public async Task<bool> ExistsAsync(int clientId, int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM wishlist WHERE id_client = @clientId AND id_product = @productId";
		cmd.AddParameter("clientId", clientId);
		cmd.AddParameter("productId", productId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task AddAsync(int clientId, int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "INSERT INTO wishlist (id_client, id_product) VALUES (@clientId, @productId)";
		cmd.AddParameter("clientId", clientId);
		cmd.AddParameter("productId", productId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task RemoveAsync(int clientId, int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM wishlist WHERE id_client = @clientId AND id_product = @productId";
		cmd.AddParameter("clientId", clientId);
		cmd.AddParameter("productId", productId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
