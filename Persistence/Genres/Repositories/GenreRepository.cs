using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Genres;
using ColeccionaloYa.Persistence.Genres.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Genres.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class GenreRepository : IGenreRepository {
	private readonly ICConnection _Connection;

	public GenreRepository(ICConnection connection) {
		_Connection = connection;
	}

	private static Genre Map(ICDataReader rs) {
		return new Genre {
			Id = rs.GetValue<int>("id_genre"),
			Name = rs.GetValue<string>("name"),
		};
	}

	public async Task<List<Genre>> GetAllAsync(CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT g.id_genre, g.name
            FROM genre g
            ORDER BY g.name";
		return await cmd.ExecuteSelectList<Genre>(Map, cancellationToken);
	}

	public async Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT g.id_genre, g.name
            FROM genre g
            WHERE g.id_genre = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteSelect<Genre>(Map, cancellationToken);
	}

	public async Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM genre g
            WHERE LOWER(g.name) = LOWER(@name)
              AND (@excludeId IS NULL OR g.id_genre <> @excludeId)";
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
            FROM genre g
            WHERE g.id_genre = ANY(@ids)";
		cmd.AddParameter("ids", ids.ToArray());
		var found = await cmd.ExecuteGetValue<long>("found", cancellationToken);
		return found == ids.Distinct().Count();
	}

	public async Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM book_genre bg
            WHERE bg.id_genre = @id";
		cmd.AddParameter("id", id);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task CreateAsync(Genre genre, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO genre (name)
            VALUES (@name)
            RETURNING id_genre";
		cmd.AddParameter("name", genre.Name);

		var newId = await cmd.ExecuteGetValue<int>("id_genre", cancellationToken);
		genre.AssignId(newId);
	}

	public async Task UpdateAsync(Genre genre, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            UPDATE genre
            SET name = @name
            WHERE id_genre = @id";
		cmd.AddParameter("id", genre.Id);
		cmd.AddParameter("name", genre.Name);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task DeleteAsync(int id, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM genre WHERE id_genre = @id";
		cmd.AddParameter("id", id);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
