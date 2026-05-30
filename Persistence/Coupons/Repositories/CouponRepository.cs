using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Coupons;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using ColeccionaloYa.Persistence.Coupons.ReadModels;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Coupons.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class CouponRepository : ICouponRepository {
	private readonly ICConnection _Connection;

	public CouponRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Coupon MapCoupon(ICDataReader rs) {
		return new Coupon {
			Id = rs.GetValue<int>("id_coupon"),
			Name = rs.GetValue<string>("name"),
			Token = rs.GetValue<string>("token"),
			Description = rs.GetValue<string>("description"),
			CreationDate = rs.GetValue<DateTime>("creation_date"),
			ValidFrom = rs.GetValue<DateTime?>("valid_from"),
			ValidUntil = rs.GetValue<DateTime?>("valid_until"),
			Porcentage = rs.GetValue<decimal>("porcentage"),
			IsActive = rs.GetValue<bool>("is_active"),
			LogicalDelete = rs.GetValue<bool>("logical_delete"),
		};
	}

	public async Task<PagedData<CouponWithCount>> GetPagedAsync(int page, int pageSize, bool? isActive, string? search, CancellationToken cancellationToken) {
		var sql = @"
            SELECT COUNT(*) OVER() AS total_count,
                   c.id_coupon, c.name, c.token, c.description, c.creation_date, c.valid_from, c.valid_until,
                   c.porcentage, c.is_active, c.logical_delete,
                   (SELECT COUNT(*) FROM coupon_client cc WHERE cc.id_coupon = c.id_coupon) AS assigned_clients_count
            FROM coupon c
            WHERE c.logical_delete = FALSE
              AND (@isActive IS NULL OR c.is_active = @isActive)
              AND (@search IS NULL OR LOWER(c.name) LIKE @searchLike OR LOWER(c.token) LIKE @searchLike)
            ORDER BY c.id_coupon DESC";

		return await PaginationHelper.FetchPagedAsync<CouponWithCount>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("isActive", (object?)isActive ?? DBNull.Value);
				cmd.AddParameter("search", (object?)search ?? DBNull.Value);
				cmd.AddParameter("searchLike", search == null ? (object)DBNull.Value : $"%{search.ToLower()}%");
			},
			rs => new CouponWithCount(MapCoupon(rs), (int)rs.GetValue<long>("assigned_clients_count")),
			page,
			pageSize,
			cancellationToken);
	}

	public async Task<Coupon?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT c.id_coupon, c.name, c.token, c.description, c.creation_date, c.valid_from, c.valid_until,
                   c.porcentage, c.is_active, c.logical_delete
            FROM coupon c
            WHERE c.id_coupon = @id AND c.logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Coupon>(MapCoupon, cancellationToken);
	}

	public async Task<CouponWithClients?> GetWithClientsAsync(int id, CancellationToken cancellationToken) {
		var coupon = await GetByIdAsync(id, cancellationToken);
		if (coupon is null) {
			return null;
		}

		var clients = await GetAssignedClientsAsync(id, cancellationToken);
		return new CouponWithClients(coupon, clients);
	}

	public async Task<Coupon?> GetByTokenAsync(string token, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT c.id_coupon, c.name, c.token, c.description, c.creation_date, c.valid_from, c.valid_until,
                   c.porcentage, c.is_active, c.logical_delete
            FROM coupon c
            WHERE c.token = @token AND c.logical_delete = FALSE";
		cmd.AddParameter("token", token);
		return await cmd.ExecuteSelect<Coupon>(MapCoupon, cancellationToken);
	}

	public async Task<List<Coupon>> GetForClientAsync(int clientId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT c.id_coupon, c.name, c.token, c.description, c.creation_date, c.valid_from, c.valid_until,
                   c.porcentage, c.is_active, c.logical_delete
            FROM coupon c
            INNER JOIN coupon_client cc ON cc.id_coupon = c.id_coupon
            WHERE cc.id_client = @clientId
              AND c.logical_delete = FALSE
              AND c.is_active = TRUE
              AND (c.valid_from IS NULL OR c.valid_from <= NOW())
              AND (c.valid_until IS NULL OR c.valid_until >= NOW())
            ORDER BY c.id_coupon DESC";
		cmd.AddParameter("clientId", clientId);
		return await cmd.ExecuteSelectList<Coupon>(MapCoupon, cancellationToken);
	}

	public async Task<bool> ExistsByTokenAsync(string token, int? excludeId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1 FROM coupon c
            WHERE c.token = @token AND c.logical_delete = FALSE
              AND (@excludeId IS NULL OR c.id_coupon <> @excludeId)";
		cmd.AddParameter("token", token);
		cmd.AddParameter("excludeId", (object?)excludeId ?? DBNull.Value);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> IsAssignedAsync(int couponId, int clientId, CancellationToken cancellationToken) {
		return await ClientAssignmentExistsAsync(couponId, clientId, cancellationToken);
	}

	public async Task<bool> ClientsExistAsync(IReadOnlyCollection<int> clientIds, CancellationToken cancellationToken) {
		if (clientIds.Count == 0) {
			return true;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT COUNT(*) AS found
            FROM client c
            WHERE c.id_client = ANY(@ids) AND c.logical_delete = FALSE";
		cmd.AddParameter("ids", clientIds.ToArray());
		var found = await cmd.ExecuteGetValue<long>("found", cancellationToken);
		return found == clientIds.Distinct().Count();
	}

	public async Task<bool> AnyClientAssignedAsync(int couponId, IReadOnlyCollection<int> clientIds, CancellationToken cancellationToken) {
		if (clientIds.Count == 0) {
			return false;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM coupon_client WHERE id_coupon = @couponId AND id_client = ANY(@ids)";
		cmd.AddParameter("couponId", couponId);
		cmd.AddParameter("ids", clientIds.ToArray());
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> ClientAssignmentExistsAsync(int couponId, int clientId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM coupon_client WHERE id_coupon = @couponId AND id_client = @clientId";
		cmd.AddParameter("couponId", couponId);
		cmd.AddParameter("clientId", clientId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<List<CouponClientBrief>> GetAssignedClientsAsync(int couponId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT cl.id_client, cl.name, cl.lastname, cl.email
            FROM coupon_client cc
            INNER JOIN client cl ON cl.id_client = cc.id_client
            WHERE cc.id_coupon = @couponId
            ORDER BY cl.id_client";
		cmd.AddParameter("couponId", couponId);
		return await cmd.ExecuteSelectList<CouponClientBrief>(rs => new CouponClientBrief(
			rs.GetValue<int>("id_client"),
			rs.GetValue<string>("name"),
			rs.GetValue<string>("lastname"),
			rs.GetValue<string>("email")), cancellationToken);
	}

	public async Task CreateAsync(Coupon coupon, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO coupon (name, token, description, creation_date, valid_from, valid_until, porcentage, is_active, logical_delete)
            VALUES (@name, @token, @description, @creationDate, @validFrom, @validUntil, @porcentage, @isActive, FALSE)
            RETURNING id_coupon";
		cmd.AddParameter("name", coupon.Name);
		cmd.AddParameter("token", coupon.Token);
		cmd.AddParameter("description", coupon.Description);
		cmd.AddParameter("creationDate", coupon.CreationDate);
		cmd.AddParameter("validFrom", (object?)coupon.ValidFrom ?? DBNull.Value);
		cmd.AddParameter("validUntil", (object?)coupon.ValidUntil ?? DBNull.Value);
		cmd.AddParameter("porcentage", coupon.Porcentage);
		cmd.AddParameter("isActive", coupon.IsActive);

		var newId = await cmd.ExecuteGetValue<int>("id_coupon", cancellationToken);
		coupon.AssignId(newId);
	}

	public async Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE coupon
            SET name = @name,
                description = @description,
                valid_from = @validFrom,
                valid_until = @validUntil,
                porcentage = @porcentage,
                is_active = @isActive
            WHERE id_coupon = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", coupon.Id);
		cmd.AddParameter("name", coupon.Name);
		cmd.AddParameter("description", coupon.Description);
		cmd.AddParameter("validFrom", (object?)coupon.ValidFrom ?? DBNull.Value);
		cmd.AddParameter("validUntil", (object?)coupon.ValidUntil ?? DBNull.Value);
		cmd.AddParameter("porcentage", coupon.Porcentage);
		cmd.AddParameter("isActive", coupon.IsActive);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task LogicalDeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "UPDATE coupon SET logical_delete = TRUE WHERE id_coupon = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task AssignClientsAsync(int couponId, IReadOnlyCollection<int> clientIds, CancellationToken cancellationToken) {
		if (clientIds.Count == 0) {
			return;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO coupon_client (id_client, id_coupon)
            SELECT c, @couponId FROM UNNEST(@ids) AS c";
		cmd.AddParameter("couponId", couponId);
		cmd.AddParameter("ids", clientIds.ToArray());
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task RemoveClientAsync(int couponId, int clientId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM coupon_client WHERE id_coupon = @couponId AND id_client = @clientId";
		cmd.AddParameter("couponId", couponId);
		cmd.AddParameter("clientId", clientId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
