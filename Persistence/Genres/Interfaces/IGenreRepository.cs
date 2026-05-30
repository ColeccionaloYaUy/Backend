using ColeccionaloYa.Domain.Genres;

namespace ColeccionaloYa.Persistence.Genres.Interfaces;

public interface IGenreRepository {
	Task<List<Genre>> GetAllAsync(CancellationToken cancellationToken);
	Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken);
	Task<bool> ExistByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
	Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken);
	Task CreateAsync(Genre genre, CancellationToken cancellationToken);
	Task UpdateAsync(Genre genre, CancellationToken cancellationToken);
	Task DeleteAsync(int id, CancellationToken cancellationToken);
}
