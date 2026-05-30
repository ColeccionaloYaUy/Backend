using ColeccionaloYa.Domain.Authors;
using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Persistence.Authors.Interfaces;

public interface IAuthorRepository {
	Task<PagedData<Author>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
	Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken);
	Task<bool> ExistByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
	Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken);
	Task CreateAsync(Author author, CancellationToken cancellationToken);
	Task UpdateAsync(Author author, CancellationToken cancellationToken);
	Task DeleteAsync(int id, CancellationToken cancellationToken);
}
