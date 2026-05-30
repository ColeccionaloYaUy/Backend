using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Tags;
using ColeccionaloYa.Persistence.Tags.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Tags.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class TagRepository : ITagRepository {
	private readonly ICConnection _Connection;

	public TagRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Tag Map(ICDataReader rs) {
		return new Tag {
			Id = rs.GetValue<int>("id_tag"),
			Name = rs.GetValue<string>("name"),
			IsFranchise = rs.GetValue<bool>("is_franchise"),
		};
	}

	public async Task<List<Tag>> GetAllAsync(bool? isFranchise, string? search, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT t.id_tag, t.name, t.is_franchise
            FROM tag t
            WHERE (@isFranchise IS NULL OR t.is_franchise = @isFranchise)
              AND (@search IS NULL OR LOWER(t.name) LIKE @searchLike)
            ORDER BY t.name";
		cmd.AddParameter("isFranchise", (object?)isFranchise ?? DBNull.Value);
		cmd.AddParameter("search", (object?)search ?? DBNull.Value);
		cmd.AddParameter("searchLike", search == null ? (object)DBNull.Value : $"%{search.ToLower()}%");
		return await cmd.ExecuteSelectList<Tag>(Map, cancellationToken);
	}

	public async Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT t.id_tag, t.name, t.is_franchise
            FROM tag t
            WHERE t.id_tag = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Tag>(Map, cancellationToken);
	}

	public async Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM tag t
            WHERE LOWER(t.name) = LOWER(@name)
              AND (@excludeId IS NULL OR t.id_tag <> @excludeId)";
		cmd.AddParameter("name", name);
		cmd.AddParameter("excludeId", (object?)excludeId ?? DBNull.Value);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> ExistByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken) {
		if (ids.Count == 0) {
			return true;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT COUNT(*) AS found
            FROM tag t
            WHERE t.id_tag = ANY(@ids)";
		cmd.AddParameter("ids", ids.ToArray());
		var found = await cmd.ExecuteGetValue<long>("found", cancellationToken);
		return found == ids.Distinct().Count();
	}

	public async Task<bool> IsUsedAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM tag_product tp
            WHERE tp.id_tag = @id
            UNION ALL
            SELECT 1
            FROM menu m
            WHERE m.id_tag = @id
            LIMIT 1";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Tag tag, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO tag (name, is_franchise)
            VALUES (@name, @isFranchise)
            RETURNING id_tag";
		cmd.AddParameter("name", tag.Name);
		cmd.AddParameter("isFranchise", tag.IsFranchise);

		var newId = await cmd.ExecuteGetValue<int>("id_tag", cancellationToken);
		tag.AssignId(newId);
	}

	public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE tag
            SET name = @name,
                is_franchise = @isFranchise
            WHERE id_tag = @id";
		cmd.AddParameter("id", tag.Id);
		cmd.AddParameter("name", tag.Name);
		cmd.AddParameter("isFranchise", tag.IsFranchise);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task DeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM tag WHERE id_tag = @id";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
