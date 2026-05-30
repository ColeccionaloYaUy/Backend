using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Authors;
using ColeccionaloYa.Persistence.Authors.Interfaces;
using ColeccionaloYa.Persistence.Shared;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Authors.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class AuthorRepository : IAuthorRepository {
	private readonly ICConnection _Connection;

	public AuthorRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Author Map(ICDataReader rs) {
		return new Author {
			Id = rs.GetValue<int>("id_author"),
			Name = rs.GetValue<string>("name"),
		};
	}

	public async Task<PagedData<Author>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken) {
		var sql = @"
            SELECT COUNT(*) OVER() AS total_count,
                   a.id_author, a.name
            FROM author a
            WHERE (@search IS NULL OR LOWER(a.name) LIKE @searchLike)
            ORDER BY a.name";

		return await PaginationHelper.FetchPagedAsync<Author>(
			_Connection,
			sql,
			cmd => {
				cmd.AddParameter("search", (object?)search ?? DBNull.Value);
				cmd.AddParameter("searchLike", search == null ? (object)DBNull.Value : $"%{search.ToLower()}%");
			},
			Map,
			page,
			pageSize,
			cancellationToken
		);
	}

	public async Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT a.id_author, a.name
            FROM author a
            WHERE a.id_author = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Author>(Map, cancellationToken);
	}

	public async Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM author a
            WHERE LOWER(a.name) = LOWER(@name)
              AND (@excludeId IS NULL OR a.id_author <> @excludeId)";
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
            FROM author a
            WHERE a.id_author = ANY(@ids)";
		cmd.AddParameter("ids", ids.ToArray());
		var found = await cmd.ExecuteGetValue<long>("found", cancellationToken);
		return found == ids.Distinct().Count();
	}

	public async Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM book_author ba
            WHERE ba.id_author = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Author author, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO author (name)
            VALUES (@name)
            RETURNING id_author";
		cmd.AddParameter("name", author.Name);

		var newId = await cmd.ExecuteGetValue<int>("id_author", cancellationToken);
		author.AssignId(newId);
	}

	public async Task UpdateAsync(Author author, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE author
            SET name = @name
            WHERE id_author = @id";
		cmd.AddParameter("id", author.Id);
		cmd.AddParameter("name", author.Name);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task DeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM author WHERE id_author = @id";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
