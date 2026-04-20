using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Clients;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Clients.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class ClientRepository : IClientRepository {
	private readonly ICConnection _Connection;

	public ClientRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Client Map(ICDataReader rs) {
		return new Client {
			Id = rs.GetValue<int>("id_client"),
			Name = rs.GetValue<string>("name"),
			Lastname = rs.GetValue<string>("lastname"),
			Email = rs.GetValue<string>("email"),
			Phone = rs.GetValue<string?>("phone"),
			PasswordHash = rs.GetValue<string>("password_hash"),
			AddressDeliveryId = rs.GetValue<int?>("address_delivery_id"),
			AddressOrderId = rs.GetValue<int?>("address_order_id"),
			RoleId = rs.GetValue<int>("role_id"),
			RoleName = rs.GetValue<string>("role_name"),
			Active = rs.GetValue<bool>("active"),
			CreationDate = rs.GetValue<DateTime>("creation_date"),
			LogicalDelete = rs.GetValue<bool>("logical_delete"),
		};
	}

	public async Task<PagedData<Client>> GetPagedAsync(int page, int pageSize, string? search, int? roleId, bool? active, CancellationToken cancellationToken) {
		var sql = @"
            SELECT COUNT(*) OVER() AS total_count,
                   c.id_client, c.name, c.lastname, c.email, c.phone, c.password_hash,
                   c.address_delivery_id, c.address_order_id,
                   c.role_id, r.name AS role_name,
                   c.active, c.creation_date, c.logical_delete
            FROM client c
            INNER JOIN roles r ON r.id = c.role_id
            WHERE c.logical_delete = FALSE
              AND (@search IS NULL OR (LOWER(c.name) LIKE @searchLike OR LOWER(c.lastname) LIKE @searchLike OR LOWER(c.email) LIKE @searchLike))
              AND (@roleId IS NULL OR c.role_id = @roleId)
              AND (@active IS NULL OR c.active = @active)
            ORDER BY c.id_client";

		return await PaginationHelper.FetchPagedAsync<Client>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("search", (object?)search ?? DBNull.Value);
				cmd.AddParameter("searchLike", search == null ? (object)DBNull.Value : $"%{search.ToLower()}%");
				cmd.AddParameter("roleId", (object?)roleId ?? DBNull.Value);
				cmd.AddParameter("active", (object?)active ?? DBNull.Value);
			},
			Map,
			page,
			pageSize,
			cancellationToken
		);
	}

	public async Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT c.id_client, c.name, c.lastname, c.email, c.phone, c.password_hash,
                   c.address_delivery_id, c.address_order_id,
                   c.role_id, r.name AS role_name,
                   c.active, c.creation_date, c.logical_delete
            FROM client c
            INNER JOIN roles r ON r.id = c.role_id
            WHERE c.id_client = @id AND c.logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Client>(Map, cancellationToken);
	}

	public async Task<bool> ExistsByEmailAsync(string email, int? excludeId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM client c
            WHERE LOWER(c.email) = LOWER(@email)
              AND c.logical_delete = FALSE
              AND (@excludeId IS NULL OR c.id_client <> @excludeId)";
		cmd.AddParameter("email", email);
		cmd.AddParameter("excludeId", (object?)excludeId ?? DBNull.Value);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Client client, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO client (name, lastname, email, phone, password_hash, role_id, active, creation_date, logical_delete)
            VALUES (@name, @lastname, @email, @phone, @hash, @roleId, @active, @creationDate, @logicalDelete)
            RETURNING id_client";
		cmd.AddParameter("name", client.Name);
		cmd.AddParameter("lastname", client.Lastname);
		cmd.AddParameter("email", client.Email);
		cmd.AddParameter("phone", (object?)client.Phone ?? DBNull.Value);
		cmd.AddParameter("hash", client.PasswordHash);
		cmd.AddParameter("roleId", client.RoleId);
		cmd.AddParameter("active", client.Active);
		cmd.AddParameter("creationDate", client.CreationDate);
		cmd.AddParameter("logicalDelete", client.LogicalDelete);

		var newId = await cmd.ExecuteGetValue<int>("id_client", cancellationToken);
		client.AssignId(newId);
	}

	public async Task UpdateProfileAsync(Client client, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE client
            SET name = @name,
                lastname = @lastname,
                phone = @phone
            WHERE id_client = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", client.Id);
		cmd.AddParameter("name", client.Name);
		cmd.AddParameter("lastname", client.Lastname);
		cmd.AddParameter("phone", (object?)client.Phone ?? DBNull.Value);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task UpdateByAdminAsync(Client client, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE client
            SET name = @name,
                lastname = @lastname,
                phone = @phone,
                role_id = @roleId,
                active = @active
            WHERE id_client = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", client.Id);
		cmd.AddParameter("name", client.Name);
		cmd.AddParameter("lastname", client.Lastname);
		cmd.AddParameter("phone", (object?)client.Phone ?? DBNull.Value);
		cmd.AddParameter("roleId", client.RoleId);
		cmd.AddParameter("active", client.Active);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task UpdateActiveAsync(int id, bool active, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE client
            SET active = @active
            WHERE id_client = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", id);
		cmd.AddParameter("active", active);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task LogicalDeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE client
            SET logical_delete = TRUE,
                active = FALSE
            WHERE id_client = @id AND logical_delete = FALSE";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
