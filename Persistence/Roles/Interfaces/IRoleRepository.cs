using ColeccionaloYa.Domain.Roles;

namespace ColeccionaloYa.Persistence.Roles.Interfaces;

public interface IRoleRepository {
	Task<List<Role>> GetAllAsync(CancellationToken cancellationToken);
	Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken);
	Task<bool> HasAssignedClientsAsync(int id, CancellationToken cancellationToken);
	Task CreateAsync(Role role, CancellationToken cancellationToken);
	Task UpdateAsync(Role role, CancellationToken cancellationToken);
	Task DeleteAsync(int id, CancellationToken cancellationToken);
}
