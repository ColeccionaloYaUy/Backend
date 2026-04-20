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

	private static void Map(Role obj, ICDataReader rs) {
		obj.Id = rs.GetValue<int>("id");
		obj.Name = rs.GetValue<string>("name");
		obj.Description = rs.GetValue<string?>("description");
	}

	public async Task<List<Role>> GetAllAsync() {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT r.id, r.name, r.description
            FROM roles r
            ORDER BY r.id";
		return await cmd.ExecuteSelectList<Role>(Map);
	}

	public async Task<Role?> GetByIdAsync(int id) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT r.id, r.name, r.description
            FROM roles r
            WHERE r.id = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Role>(Map);
	}

	public async Task<bool> ExistsByNameAsync(string name, int? excludeId) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM roles r
            WHERE LOWER(r.name) = LOWER(@name)
              AND (@excludeId IS NULL OR r.id <> @excludeId)";
		cmd.AddParameter("name", name);
		cmd.AddParameter("excludeId", (object?)excludeId ?? DBNull.Value);
		return await cmd.ExecuteCommandExists();
	}

	public async Task<bool> HasAssignedClientsAsync(int id) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM client c
            WHERE c.role_id = @id AND c.logical_delete = FALSE";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists();
	}

	public async Task CreateAsync(Role role) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO roles (name, description)
            VALUES (@name, @description)
            RETURNING id";
		cmd.AddParameter("name", role.Name);
		cmd.AddParameter("description", (object?)role.Description ?? DBNull.Value);

		var newId = await cmd.ExecuteGetValue<int>("id");
		role.AssignId(newId);
	}

	public async Task UpdateAsync(Role role) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE roles
            SET name = @name,
                description = @description
            WHERE id = @id";
		cmd.AddParameter("id", role.Id);
		cmd.AddParameter("name", role.Name);
		cmd.AddParameter("description", (object?)role.Description ?? DBNull.Value);

		await cmd.ExecuteCommandNonQuery();
	}

	public async Task DeleteAsync(int id) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM roles WHERE id = @id";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery();
	}
}
