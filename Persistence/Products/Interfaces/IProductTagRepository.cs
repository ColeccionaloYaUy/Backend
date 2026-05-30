using ColeccionaloYa.Domain.Tags;

namespace ColeccionaloYa.Persistence.Products.Interfaces;

public interface IProductTagRepository {
	Task<List<Tag>> GetTagsAsync(int productId, CancellationToken cancellationToken);
	Task<bool> AnyLinkedAsync(int productId, IReadOnlyCollection<int> tagIds, CancellationToken cancellationToken);
	Task<bool> LinkExistsAsync(int productId, int tagId, CancellationToken cancellationToken);
	Task AddAsync(int productId, IReadOnlyCollection<int> tagIds, CancellationToken cancellationToken);
	Task RemoveAsync(int productId, int tagId, CancellationToken cancellationToken);
}
