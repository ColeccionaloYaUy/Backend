using ColeccionaloYa.Domain.Menus;

namespace ColeccionaloYa.Persistence.Menus.Interfaces;

public interface IMenuRepository {
	Task<List<MenuItem>> GetAllAsync(CancellationToken cancellationToken);
	Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
	Task<bool> TagExistsAsync(int tagId, CancellationToken cancellationToken);
	Task<bool> HasChildrenAsync(int id, CancellationToken cancellationToken);
	Task<bool> WouldCreateCycleAsync(int id, int newParentId, CancellationToken cancellationToken);
	Task CreateAsync(MenuItem item, CancellationToken cancellationToken);
	Task UpdateAsync(MenuItem item, CancellationToken cancellationToken);
	Task ReorderAsync(IReadOnlyCollection<(int Id, int Order)> items, CancellationToken cancellationToken);
	Task DeleteAsync(int id, CancellationToken cancellationToken);
}
