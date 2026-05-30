using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Domain.Tags;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Persistence.Products.ReadModels;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Products.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class ProductRepository : IProductRepository {
	private readonly ICConnection _Connection;

	public ProductRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Product MapProduct(ICDataReader rs) {
		return new Product {
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
	}

	private static ProductCatalogItem MapCatalogItem(ICDataReader rs) {
		var discountId = rs.GetValue<int?>("discount_id");
		ActiveDiscountInfo? discount = discountId.HasValue
			? new ActiveDiscountInfo(
				discountId.Value,
				rs.GetValue<decimal>("discount_porcentage"),
				rs.GetValue<DateTime?>("discount_valid_from"),
				rs.GetValue<DateTime?>("discount_valid_until"),
				rs.GetValue<decimal>("discount_final_price"))
			: null;

		return new ProductCatalogItem(
			rs.GetValue<int>("id_product"),
			rs.GetValue<string>("name"),
			rs.GetValue<string>("short_description"),
			rs.GetValue<decimal>("price"),
			rs.GetValue<string>("type"),
			rs.GetValue<decimal>("weight"),
			rs.GetValue<bool>("is_featured"),
			rs.GetValue<DateOnly>("creation_date"),
			rs.GetValue<string?>("cover_url"),
			rs.GetValue<int>("current_stock"),
			discount,
			rs.GetValue<string?>("type_book"));
	}

	private static string BuildOrderBy(string sort) {
		return sort switch {
			"price_asc" => "p.price ASC, p.id_product ASC",
			"price_desc" => "p.price DESC, p.id_product ASC",
			"name" => "p.name ASC, p.id_product ASC",
			"best_sellers" => "(SELECT COALESCE(SUM(ol.quantity), 0) FROM order_line ol WHERE ol.id_product = p.id_product) DESC, p.id_product DESC",
			_ => "p.creation_date DESC, p.id_product DESC", // newest
		};
	}

	public async Task<PagedData<ProductCatalogItem>> GetCatalogAsync(ProductsFilter filter, CancellationToken cancellationToken) {
		var sql = $@"
            SELECT COUNT(*) OVER() AS total_count,
                   p.id_product, p.name, p.short_description, p.price, p.type::text AS type,
                   p.weight, p.is_featured, p.creation_date,
                   (SELECT pg.url FROM product_gallery pg WHERE pg.id_product = p.id_product ORDER BY pg.""order"" ASC LIMIT 1) AS cover_url,
                   COALESCE((SELECT SUM(s.input) - SUM(s.output) FROM stock s WHERE s.id_product = p.id_product), 0) AS current_stock,
                   d.id_discount AS discount_id,
                   d.porcentage AS discount_porcentage,
                   d.valid_from AS discount_valid_from,
                   d.valid_until AS discount_valid_until,
                   (p.price * (1 - d.porcentage / 100)) AS discount_final_price,
                   b.type::text AS type_book
            FROM product p
            LEFT JOIN book b ON b.id_product = p.id_product
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
            WHERE p.logical_delete = FALSE
              AND (@search IS NULL OR (LOWER(p.name) LIKE @searchLike OR LOWER(p.short_description) LIKE @searchLike))
              AND (@type IS NULL OR p.type = @type::product_type)
              AND (@typeBook IS NULL OR (p.type = 'book'::product_type AND b.type = @typeBook::type_book))
              AND (array_length(@tags, 1) IS NULL OR EXISTS (SELECT 1 FROM tag_product tp WHERE tp.id_product = p.id_product AND tp.id_tag = ANY(@tags)))
              AND (array_length(@genres, 1) IS NULL OR EXISTS (SELECT 1 FROM book_genre bg WHERE bg.id_book = p.id_product AND bg.id_genre = ANY(@genres)))
              AND (array_length(@authors, 1) IS NULL OR EXISTS (SELECT 1 FROM book_author ba WHERE ba.id_book = p.id_product AND ba.id_author = ANY(@authors)))
              AND (@franchise IS NULL OR EXISTS (SELECT 1 FROM tag_product tp WHERE tp.id_product = p.id_product AND tp.id_tag = @franchise))
              AND (@minPrice IS NULL OR p.price >= @minPrice)
              AND (@maxPrice IS NULL OR p.price <= @maxPrice)
              AND (@inStock IS NULL OR @inStock = FALSE OR COALESCE((SELECT SUM(s.input) - SUM(s.output) FROM stock s WHERE s.id_product = p.id_product), 0) > 0)
              AND (@onSale IS NULL OR @onSale = FALSE OR d.id_discount IS NOT NULL)
              AND (@featured IS NULL OR p.is_featured = @featured)
              AND (@newArrivals = FALSE OR p.creation_date >= (CURRENT_DATE - INTERVAL '30 days'))
              AND (@relatedTo IS NULL OR (p.id_product <> @relatedTo AND EXISTS (
                    SELECT 1 FROM tag_product tp1
                    JOIN tag_product tp2 ON tp1.id_tag = tp2.id_tag
                    WHERE tp1.id_product = p.id_product AND tp2.id_product = @relatedTo)))
            ORDER BY {BuildOrderBy(filter.Sort)}";

		return await PaginationHelper.FetchPagedAsync<ProductCatalogItem>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("search", (object?)filter.Search ?? DBNull.Value);
				cmd.AddParameter("searchLike", filter.Search == null ? (object)DBNull.Value : $"%{filter.Search.ToLower()}%");
				cmd.AddParameter("type", (object?)filter.Type ?? DBNull.Value);
				cmd.AddParameter("typeBook", (object?)filter.TypeBook ?? DBNull.Value);
				cmd.AddParameter("tags", filter.Tags.ToArray());
				cmd.AddParameter("genres", filter.Genres.ToArray());
				cmd.AddParameter("authors", filter.Authors.ToArray());
				cmd.AddParameter("franchise", (object?)filter.Franchise ?? DBNull.Value);
				cmd.AddParameter("minPrice", (object?)filter.MinPrice ?? DBNull.Value);
				cmd.AddParameter("maxPrice", (object?)filter.MaxPrice ?? DBNull.Value);
				cmd.AddParameter("inStock", (object?)filter.InStock ?? DBNull.Value);
				cmd.AddParameter("onSale", (object?)filter.OnSale ?? DBNull.Value);
				cmd.AddParameter("featured", (object?)filter.Featured ?? DBNull.Value);
				cmd.AddParameter("newArrivals", filter.NewArrivals);
				cmd.AddParameter("relatedTo", (object?)filter.RelatedTo ?? DBNull.Value);
			},
			MapCatalogItem,
			filter.Page,
			filter.PageSize,
			cancellationToken);
	}

	public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT p.id_product, p.name, p.short_description, p.long_description, p.price,
                   p.type::text AS type, p.weight, p.is_featured, p.creation_date, p.logical_delete
            FROM product p
            WHERE p.id_product = @id AND p.logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Product>(MapProduct, cancellationToken);
	}

	public async Task<ProductDetailData?> GetDetailAsync(int id, CancellationToken cancellationToken) {
		var product = await GetByIdAsync(id, cancellationToken);
		if (product is null) {
			return null;
		}

		var galleryCmd = _Connection.CreateCommand();
		galleryCmd.CommandText = @"
            SELECT pg.id_gallery, pg.id_product, pg.""order"" AS order_value, pg.url
            FROM product_gallery pg
            WHERE pg.id_product = @id
            ORDER BY pg.""order"" ASC";
		galleryCmd.AddParameter("id", id);
		var gallery = await galleryCmd.ExecuteSelectList<ProductGalleryImage>(rs => new ProductGalleryImage(
			rs.GetValue<int>("id_gallery"),
			rs.GetValue<int>("id_product"),
			rs.GetValue<int>("order_value"),
			rs.GetValue<string>("url")), cancellationToken);

		var tagsCmd = _Connection.CreateCommand();
		tagsCmd.CommandText = @"
            SELECT t.id_tag, t.name, t.is_franchise
            FROM tag_product tp
            INNER JOIN tag t ON t.id_tag = tp.id_tag
            WHERE tp.id_product = @id
            ORDER BY t.name";
		tagsCmd.AddParameter("id", id);
		var tags = await tagsCmd.ExecuteSelectList<Tag>(rs => new Tag {
			Id = rs.GetValue<int>("id_tag"),
			Name = rs.GetValue<string>("name"),
			IsFranchise = rs.GetValue<bool>("is_franchise"),
		}, cancellationToken);

		var discountCmd = _Connection.CreateCommand();
		discountCmd.CommandText = @"
            SELECT dd.id_discount, dd.porcentage, dd.valid_from, dd.valid_until,
                   (@price * (1 - dd.porcentage / 100)) AS final_price
            FROM discount dd
            WHERE dd.id_product = @id
              AND dd.is_active = TRUE AND dd.logical_delete = FALSE
              AND (dd.valid_from IS NULL OR dd.valid_from <= NOW())
              AND (dd.valid_until IS NULL OR dd.valid_until >= NOW())
            ORDER BY dd.porcentage DESC
            LIMIT 1";
		discountCmd.AddParameter("id", id);
		discountCmd.AddParameter("price", product.Price);
		var discount = await discountCmd.ExecuteSelect<ActiveDiscountInfo>(rs => new ActiveDiscountInfo(
			rs.GetValue<int>("id_discount"),
			rs.GetValue<decimal>("porcentage"),
			rs.GetValue<DateTime?>("valid_from"),
			rs.GetValue<DateTime?>("valid_until"),
			rs.GetValue<decimal>("final_price")), cancellationToken);

		var stockCmd = _Connection.CreateCommand();
		stockCmd.CommandText = @"
            SELECT COALESCE(SUM(s.input) - SUM(s.output), 0) AS current_stock,
                   MAX(s.date) AS last_movement_date
            FROM stock s
            WHERE s.id_product = @id";
		stockCmd.AddParameter("id", id);
		var stock = await stockCmd.ExecuteSelect<ProductStockInfo>(rs => new ProductStockInfo(
			rs.GetValue<int>("current_stock"),
			rs.GetValue<DateTime?>("last_movement_date")), cancellationToken)
			?? new ProductStockInfo(0, null);

		return new ProductDetailData(product, gallery, tags, discount, stock);
	}

	public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM product WHERE id_product = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<string?> GetTypeAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT type::text AS type FROM product WHERE id_product = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", id);
		var row = await cmd.ExecuteSelect<string>(rs => rs.GetValue<string>("type"), cancellationToken);
		return row;
	}

	public async Task<bool> IsInUseAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM order_line ol
            INNER JOIN ""order"" o ON o.id_order = ol.id_order
            WHERE ol.id_product = @id
              AND o.status IN ('pending', 'confirmed', 'processing', 'shipped')
            UNION ALL
            SELECT 1
            FROM pack_product pp
            INNER JOIN product pk ON pk.id_product = pp.id_pack
            WHERE pp.id_product = @id AND pk.logical_delete = FALSE
            LIMIT 1";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Product product, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO product (name, short_description, long_description, price, creation_date, type, weight, logical_delete, is_featured)
            VALUES (@name, @shortDescription, @longDescription, @price, @creationDate, @type::product_type, @weight, FALSE, @isFeatured)
            RETURNING id_product";
		cmd.AddParameter("name", product.Name);
		cmd.AddParameter("shortDescription", product.ShortDescription);
		cmd.AddParameter("longDescription", product.LongDescription);
		cmd.AddParameter("price", product.Price);
		cmd.AddParameter("creationDate", product.CreationDate);
		cmd.AddParameter("type", product.Type.ToString().ToLower());
		cmd.AddParameter("weight", product.Weight);
		cmd.AddParameter("isFeatured", product.IsFeatured);

		var newId = await cmd.ExecuteGetValue<int>("id_product", cancellationToken);
		product.AssignId(newId);
	}

	public async Task UpdateAsync(Product product, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE product
            SET name = @name,
                short_description = @shortDescription,
                long_description = @longDescription,
                price = @price,
                type = @type::product_type,
                weight = @weight,
                is_featured = @isFeatured
            WHERE id_product = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", product.Id);
		cmd.AddParameter("name", product.Name);
		cmd.AddParameter("shortDescription", product.ShortDescription);
		cmd.AddParameter("longDescription", product.LongDescription);
		cmd.AddParameter("price", product.Price);
		cmd.AddParameter("type", product.Type.ToString().ToLower());
		cmd.AddParameter("weight", product.Weight);
		cmd.AddParameter("isFeatured", product.IsFeatured);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task LogicalDeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "UPDATE product SET logical_delete = TRUE WHERE id_product = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
