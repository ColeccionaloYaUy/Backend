using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Discounts;
using ColeccionaloYa.Persistence.Discounts.Interfaces;
using ColeccionaloYa.Persistence.Discounts.ReadModels;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Discounts.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class DiscountRepository : IDiscountRepository {
	private readonly ICConnection _Connection;

	public DiscountRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Discount MapDiscount(ICDataReader rs) {
		return new Discount {
			Id = rs.GetValue<int>("id_discount"),
			ProductId = rs.GetValue<int>("id_product"),
			Porcentage = rs.GetValue<decimal>("porcentage"),
			CreationDate = rs.GetValue<DateTime>("creation_date"),
			ValidFrom = rs.GetValue<DateTime?>("valid_from"),
			ValidUntil = rs.GetValue<DateTime?>("valid_until"),
			IsActive = rs.GetValue<bool>("is_active"),
			LogicalDelete = rs.GetValue<bool>("logical_delete"),
		};
	}

	private static DiscountWithProduct MapWithProduct(ICDataReader rs) {
		return new DiscountWithProduct(MapDiscount(rs), rs.GetValue<string>("product_name"));
	}

	public async Task<PagedData<DiscountWithProduct>> GetPagedAsync(int page, int pageSize, bool? isActive, int? productId, CancellationToken cancellationToken) {
		var sql = @"
            SELECT COUNT(*) OVER() AS total_count,
                   d.id_discount, d.id_product, d.porcentage, d.creation_date, d.valid_from, d.valid_until,
                   d.is_active, d.logical_delete, p.name AS product_name
            FROM discount d
            INNER JOIN product p ON p.id_product = d.id_product
            WHERE d.logical_delete = FALSE
              AND (@isActive IS NULL OR d.is_active = @isActive)
              AND (@productId IS NULL OR d.id_product = @productId)
            ORDER BY d.id_discount DESC";

		return await PaginationHelper.FetchPagedAsync<DiscountWithProduct>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("isActive", (object?)isActive ?? DBNull.Value);
				cmd.AddParameter("productId", (object?)productId ?? DBNull.Value);
			},
			MapWithProduct,
			page,
			pageSize,
			cancellationToken);
	}

	public async Task<DiscountWithProduct?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT d.id_discount, d.id_product, d.porcentage, d.creation_date, d.valid_from, d.valid_until,
                   d.is_active, d.logical_delete, p.name AS product_name
            FROM discount d
            INNER JOIN product p ON p.id_product = d.id_product
            WHERE d.id_discount = @id AND d.logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<DiscountWithProduct>(MapWithProduct, cancellationToken);
	}

	public async Task<Discount?> GetEntityAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT d.id_discount, d.id_product, d.porcentage, d.creation_date, d.valid_from, d.valid_until,
                   d.is_active, d.logical_delete
            FROM discount d
            WHERE d.id_discount = @id AND d.logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Discount>(MapDiscount, cancellationToken);
	}

	public async Task<bool> HasOverlappingActiveAsync(int productId, DateTime? validFrom, DateTime? validUntil, int? excludeId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM discount d
            WHERE d.id_product = @productId
              AND d.is_active = TRUE
              AND d.logical_delete = FALSE
              AND (@excludeId IS NULL OR d.id_discount <> @excludeId)
              AND COALESCE(d.valid_from, '-infinity'::timestamp) <= COALESCE(@validUntil, 'infinity'::timestamp)
              AND COALESCE(d.valid_until, 'infinity'::timestamp) >= COALESCE(@validFrom, '-infinity'::timestamp)";
		cmd.AddParameter("productId", productId);
		cmd.AddParameter("validFrom", (object?)validFrom ?? DBNull.Value);
		cmd.AddParameter("validUntil", (object?)validUntil ?? DBNull.Value);
		cmd.AddParameter("excludeId", (object?)excludeId ?? DBNull.Value);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Discount discount, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO discount (id_product, porcentage, creation_date, valid_from, valid_until, is_active, logical_delete)
            VALUES (@productId, @porcentage, @creationDate, @validFrom, @validUntil, @isActive, FALSE)
            RETURNING id_discount";
		cmd.AddParameter("productId", discount.ProductId);
		cmd.AddParameter("porcentage", discount.Porcentage);
		cmd.AddParameter("creationDate", discount.CreationDate);
		cmd.AddParameter("validFrom", (object?)discount.ValidFrom ?? DBNull.Value);
		cmd.AddParameter("validUntil", (object?)discount.ValidUntil ?? DBNull.Value);
		cmd.AddParameter("isActive", discount.IsActive);

		var newId = await cmd.ExecuteGetValue<int>("id_discount", cancellationToken);
		discount.AssignId(newId);
	}

	public async Task UpdateAsync(Discount discount, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE discount
            SET porcentage = @porcentage,
                valid_from = @validFrom,
                valid_until = @validUntil,
                is_active = @isActive
            WHERE id_discount = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", discount.Id);
		cmd.AddParameter("porcentage", discount.Porcentage);
		cmd.AddParameter("validFrom", (object?)discount.ValidFrom ?? DBNull.Value);
		cmd.AddParameter("validUntil", (object?)discount.ValidUntil ?? DBNull.Value);
		cmd.AddParameter("isActive", discount.IsActive);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
