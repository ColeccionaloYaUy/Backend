using ColeccionaloYa.Domain.Tags;

namespace ColeccionaloYa.Persistence.Tags.Interfaces;

public interface ITagRepository {
	Task<List<Tag>> GetAllAsync(bool? isFranchise, string? search, CancellationToken cancellationToken);
	Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken);
	Task<bool> ExistByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
	Task<bool> IsUsedAsync(int id, CancellationToken cancellationToken);
	Task CreateAsync(Tag tag, CancellationToken cancellationToken);
	Task UpdateAsync(Tag tag, CancellationToken cancellationToken);
	Task DeleteAsync(int id, CancellationToken cancellationToken);
}
