using ColeccionaloYa.Domain.Gallery;

namespace ColeccionaloYa.Persistence.Gallery.Interfaces;

public interface IGalleryRepository {
	Task<List<GalleryImage>> GetByProductAsync(int productId, CancellationToken cancellationToken);
	Task<GalleryImage?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<int> GetNextOrderAsync(int productId, CancellationToken cancellationToken);
	Task<bool> AllBelongToProductAsync(int productId, IReadOnlyCollection<int> galleryIds, CancellationToken cancellationToken);
	Task CreateAsync(GalleryImage image, CancellationToken cancellationToken);
	Task ReorderAsync(IReadOnlyCollection<(int Id, int Order)> items, CancellationToken cancellationToken);
	Task DeleteAsync(int id, CancellationToken cancellationToken);
}
