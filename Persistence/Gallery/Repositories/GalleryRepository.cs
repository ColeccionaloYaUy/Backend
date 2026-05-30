using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Gallery;
using ColeccionaloYa.Persistence.Gallery.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Gallery.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class GalleryRepository : IGalleryRepository {
	private readonly ICConnection _Connection;

	public GalleryRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static GalleryImage Map(ICDataReader rs) {
		return new GalleryImage {
			Id = rs.GetValue<int>("id_gallery"),
			ProductId = rs.GetValue<int>("id_product"),
			Order = rs.GetValue<int>("order_value"),
			Url = rs.GetValue<string>("url"),
		};
	}

	public async Task<List<GalleryImage>> GetByProductAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT pg.id_gallery, pg.id_product, pg.""order"" AS order_value, pg.url
            FROM product_gallery pg
            WHERE pg.id_product = @productId
            ORDER BY pg.""order"" ASC";
		cmd.AddParameter("productId", productId);
		return await cmd.ExecuteSelectList<GalleryImage>(Map, cancellationToken);
	}

	public async Task<GalleryImage?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT pg.id_gallery, pg.id_product, pg.""order"" AS order_value, pg.url
            FROM product_gallery pg
            WHERE pg.id_gallery = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<GalleryImage>(Map, cancellationToken);
	}

	public async Task<int> GetNextOrderAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT COALESCE(MAX(pg.""order""), 0) + 1 AS next_order
            FROM product_gallery pg
            WHERE pg.id_product = @productId";
		cmd.AddParameter("productId", productId);
		return await cmd.ExecuteGetValue<int>("next_order", cancellationToken);
	}

	public async Task<bool> AllBelongToProductAsync(int productId, IReadOnlyCollection<int> galleryIds, CancellationToken cancellationToken) {
		if (galleryIds.Count == 0) {
			return true;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT COUNT(*) AS found
            FROM product_gallery pg
            WHERE pg.id_product = @productId AND pg.id_gallery = ANY(@ids)";
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("ids", galleryIds.ToArray());
		var found = await cmd.ExecuteGetValue<long>("found", cancellationToken);
		return found == galleryIds.Distinct().Count();
	}

	public async Task CreateAsync(GalleryImage image, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO product_gallery (id_product, ""order"", url)
            VALUES (@productId, @order, @url)
            RETURNING id_gallery";
		cmd.AddParameter("productId", image.ProductId);
		cmd.AddParameter("order", image.Order);
		cmd.AddParameter("url", image.Url);

		var newId = await cmd.ExecuteGetValue<int>("id_gallery", cancellationToken);
		image.AssignId(newId);
	}

	public async Task ReorderAsync(IReadOnlyCollection<(int Id, int Order)> items, CancellationToken cancellationToken) {
		foreach (var (id, order) in items) {
			var cmd = _Connection.CreateCommand();
			cmd.CommandText = @"UPDATE product_gallery SET ""order"" = @order WHERE id_gallery = @id";
			cmd.AddParameter("id", id);
			cmd.AddParameter("order", order);
			await cmd.ExecuteCommandNonQuery(cancellationToken);
		}
	}

	public async Task DeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM product_gallery WHERE id_gallery = @id";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
