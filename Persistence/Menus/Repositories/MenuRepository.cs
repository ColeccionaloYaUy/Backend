using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Menus;
using ColeccionaloYa.Persistence.Menus.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Menus.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class MenuRepository : IMenuRepository {
	private readonly ICConnection _Connection;

	public MenuRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static MenuItem Map(ICDataReader rs) {
		return new MenuItem {
			Id = rs.GetValue<int>("id_menu"),
			Name = rs.GetValue<string>("name"),
			TagId = rs.GetValue<int>("id_tag"),
			Order = rs.GetValue<int>("order_value"),
			MenuReferencedId = rs.GetValue<int?>("id_menu_referenced"),
			IsCategoryFilter = rs.GetValue<bool>("is_category_filter"),
		};
	}

	public async Task<List<MenuItem>> GetAllAsync(CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT m.id_menu, m.name, m.id_tag, m.""order"" AS order_value, m.id_menu_referenced, m.is_category_filter
            FROM menu m
            ORDER BY m.""order"" ASC, m.id_menu ASC";
		return await cmd.ExecuteSelectList<MenuItem>(Map, cancellationToken);
	}

	public async Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT m.id_menu, m.name, m.id_tag, m.""order"" AS order_value, m.id_menu_referenced, m.is_category_filter
            FROM menu m
            WHERE m.id_menu = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<MenuItem>(Map, cancellationToken);
	}

	public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM menu WHERE id_menu = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> TagExistsAsync(int tagId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM tag WHERE id_tag = @id";
		cmd.AddParameter("id", tagId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> HasChildrenAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM menu WHERE id_menu_referenced = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> WouldCreateCycleAsync(int id, int newParentId, CancellationToken cancellationToken) {
		if (id == newParentId) {
			return true;
		}

		// Walk up the ancestor chain from newParentId; if we reach id, attaching id under newParentId creates a cycle.
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            WITH RECURSIVE ancestors AS (
                SELECT id_menu, id_menu_referenced
                FROM menu
                WHERE id_menu = @newParentId
                UNION ALL
                SELECT m.id_menu, m.id_menu_referenced
                FROM menu m
                INNER JOIN ancestors a ON m.id_menu = a.id_menu_referenced
            )
            SELECT 1 FROM ancestors WHERE id_menu = @id";
		cmd.AddParameter("id", id);
		cmd.AddParameter("newParentId", newParentId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(MenuItem item, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO menu (name, id_tag, ""order"", id_menu_referenced, is_category_filter)
            VALUES (@name, @tagId, @order, @parentId, @isCategoryFilter)
            RETURNING id_menu";
		cmd.AddParameter("name", item.Name);
		cmd.AddParameter("tagId", item.TagId);
		cmd.AddParameter("order", item.Order);
		cmd.AddParameter("parentId", (object?)item.MenuReferencedId ?? DBNull.Value);
		cmd.AddParameter("isCategoryFilter", item.IsCategoryFilter);

		var newId = await cmd.ExecuteGetValue<int>("id_menu", cancellationToken);
		item.AssignId(newId);
	}

	public async Task UpdateAsync(MenuItem item, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE menu
            SET name = @name,
                id_tag = @tagId,
                ""order"" = @order,
                id_menu_referenced = @parentId,
                is_category_filter = @isCategoryFilter
            WHERE id_menu = @id";
		cmd.AddParameter("id", item.Id);
		cmd.AddParameter("name", item.Name);
		cmd.AddParameter("tagId", item.TagId);
		cmd.AddParameter("order", item.Order);
		cmd.AddParameter("parentId", (object?)item.MenuReferencedId ?? DBNull.Value);
		cmd.AddParameter("isCategoryFilter", item.IsCategoryFilter);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task ReorderAsync(IReadOnlyCollection<(int Id, int Order)> items, CancellationToken cancellationToken) {
		foreach (var (id, order) in items) {
			var cmd = _Connection.CreateCommand();
			cmd.CommandText = @"UPDATE menu SET ""order"" = @order WHERE id_menu = @id";
			cmd.AddParameter("id", id);
			cmd.AddParameter("order", order);
			await cmd.ExecuteCommandNonQuery(cancellationToken);
		}
	}

	public async Task DeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM menu WHERE id_menu = @id";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
