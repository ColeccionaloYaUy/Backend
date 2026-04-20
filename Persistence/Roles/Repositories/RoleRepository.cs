using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Roles;
using ColeccionaloYa.Persistence.Roles.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Roles.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class RoleRepository : IRoleRepository {
	private readonly ICConnection _Connection;

	public RoleRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Role Map(ICDataReader rs) {
		return new Role {
			Id = rs.GetValue<int>("id"),
			Name = rs.GetValue<string>("name"),
			Description = rs.GetValue<string?>("description"),
		};
	}

	public async Task<List<Role>> GetAllAsync(CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT r.id, r.name, r.description
            FROM roles r
            ORDER BY r.id";
		return await cmd.ExecuteSelectList<Role>(Map, cancellationToken);
	}

	public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT r.id, r.name, r.description
            FROM roles r
            WHERE r.id = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Role>(Map, cancellationToken);
	}

	public async Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM roles r
            WHERE LOWER(r.name) = LOWER(@name)
              AND (@excludeId IS NULL OR r.id <> @excludeId)";
		cmd.AddParameter("name", name);
		cmd.AddParameter("excludeId", (object?)excludeId ?? DBNull.Value);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> HasAssignedClientsAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM client c
            WHERE c.role_id = @id AND c.logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Role role, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO roles (name, description)
            VALUES (@name, @description)
            RETURNING id";
		cmd.AddParameter("name", role.Name);
		cmd.AddParameter("description", (object?)role.Description ?? DBNull.Value);

		var newId = await cmd.ExecuteGetValue<int>("id", cancellationToken);
		role.AssignId(newId);
	}

	public async Task UpdateAsync(Role role, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE roles
            SET name = @name,
                description = @description
            WHERE id = @id";
		cmd.AddParameter("id", role.Id);
		cmd.AddParameter("name", role.Name);
		cmd.AddParameter("description", (object?)role.Description ?? DBNull.Value);

		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task DeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM roles WHERE id = @id";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
