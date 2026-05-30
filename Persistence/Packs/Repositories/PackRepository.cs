using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Packs;
using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Persistence.Packs.Interfaces;
using ColeccionaloYa.Persistence.Packs.ReadModels;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Packs.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class PackRepository : IPackRepository {
	private readonly ICConnection _Connection;

	public PackRepository(ICConnection connection) {
		_Connection = connection;
	}

	public async Task<bool> ExistsAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM pack pk
            INNER JOIN product p ON p.id_product = pk.id_product
            WHERE pk.id_product = @id AND p.logical_delete = FALSE";
		cmd.AddParameter("id", productId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<PackDetailData?> GetDetailAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT p.id_product, p.name, p.short_description, p.long_description, p.price,
                   p.type::text AS type, p.weight, p.is_featured, p.creation_date, p.logical_delete,
                   pk.product_count, pk.creation_date AS pack_creation_date
            FROM pack pk
            INNER JOIN product p ON p.id_product = pk.id_product
            WHERE pk.id_product = @id AND p.logical_delete = FALSE";
		cmd.AddParameter("id", productId);

		var header = await cmd.ExecuteSelect(rs => {
			var product = new Product {
				Id = rs.GetValue<int>("id_product"),
				Name = rs.GetValue<string>("name"),
				ShortDescription = rs.GetValue<string>("short_description"),
				LongDescription = rs.GetValue<string>("long_description"),
				Price = rs.GetValue<decimal>("price"),
				Type = Enum.Parse<ProductType>(rs.GetValue<string>("type"), ignoreCase: true),
				Weight = rs.GetValue<decimal>("weight"),
				IsFeatured = rs.GetValue<bool>("is_featured"),
				CreationDate = rs.GetValue<DateOnly>("creation_date"),
				LogicalDelete = rs.GetValue<bool>("logical_delete"),
			};
			var pack = new Pack {
				ProductId = product.Id,
				ProductCount = rs.GetValue<int>("product_count"),
				CreationDate = rs.GetValue<DateTime>("pack_creation_date"),
			};
			return new PackDetailData(pack, product, new List<PackProductItem>());
		}, cancellationToken);

		if (header is null) {
			return null;
		}

		var itemsCmd = _Connection.CreateCommand();
		itemsCmd.CommandText = @"
            SELECT pp.id_product, p.name, p.price, p.type::text AS type, pp.quantity,
                   (SELECT pg.url FROM product_gallery pg WHERE pg.id_product = p.id_product ORDER BY pg.""order"" ASC LIMIT 1) AS cover_url
            FROM pack_product pp
            INNER JOIN product p ON p.id_product = pp.id_product
            WHERE pp.id_pack = @id
            ORDER BY p.name";
		itemsCmd.AddParameter("id", productId);
		var items = await itemsCmd.ExecuteSelectList<PackProductItem>(rs => new PackProductItem(
			new PackProductRef(
				rs.GetValue<int>("id_product"),
				rs.GetValue<string>("name"),
				rs.GetValue<decimal>("price"),
				rs.GetValue<string>("type"),
				rs.GetValue<string?>("cover_url")),
			rs.GetValue<int>("quantity")), cancellationToken);

		return header with { Products = items };
	}

	public async Task<bool> AllValidNonPackProductsAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken) {
		if (productIds.Count == 0) {
			return true;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT COUNT(*) AS found
            FROM product p
            WHERE p.id_product = ANY(@ids) AND p.logical_delete = FALSE AND p.type <> 'pack'::product_type";
		cmd.AddParameter("ids", productIds.ToArray());
		var found = await cmd.ExecuteGetValue<long>("found", cancellationToken);
		return found == productIds.Distinct().Count();
	}

	public async Task<bool> IsValidNonPackProductAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM product p
            WHERE p.id_product = @id AND p.logical_delete = FALSE AND p.type <> 'pack'::product_type";
		cmd.AddParameter("id", productId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> ItemExistsAsync(int packId, int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM pack_product WHERE id_pack = @packId AND id_product = @productId";
		cmd.AddParameter("packId", packId);
		cmd.AddParameter("productId", productId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Pack pack, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO pack (product_count, creation_date, id_product)
            VALUES (@count, @creationDate, @id)";
		cmd.AddParameter("count", pack.ProductCount);
		cmd.AddParameter("creationDate", pack.CreationDate);
		cmd.AddParameter("id", pack.ProductId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task AddItemsAsync(int packId, IReadOnlyCollection<(int ProductId, int Quantity)> items, CancellationToken cancellationToken) {
		foreach (var (productId, quantity) in items) {
			await AddItemAsync(packId, productId, quantity, cancellationToken);
		}
	}

	public async Task AddItemAsync(int packId, int productId, int quantity, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO pack_product (id_pack, id_product, quantity)
            VALUES (@packId, @productId, @quantity)";
		cmd.AddParameter("packId", packId);
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("quantity", quantity);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task UpdateItemAsync(int packId, int productId, int quantity, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE pack_product SET quantity = @quantity
            WHERE id_pack = @packId AND id_product = @productId";
		cmd.AddParameter("packId", packId);
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("quantity", quantity);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task RemoveItemAsync(int packId, int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM pack_product WHERE id_pack = @packId AND id_product = @productId";
		cmd.AddParameter("packId", packId);
		cmd.AddParameter("productId", productId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task RecalculateCountAsync(int packId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE pack
            SET product_count = COALESCE((SELECT SUM(pp.quantity) FROM pack_product pp WHERE pp.id_pack = @packId), 0)
            WHERE id_product = @packId";
		cmd.AddParameter("packId", packId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
