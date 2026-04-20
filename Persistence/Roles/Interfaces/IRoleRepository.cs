using ColeccionaloYa.Domain.Roles;

namespace ColeccionaloYa.Persistence.Roles.Interfaces;

public interface IRoleRepository {
	Task<List<Role>> GetAllAsync();
	Task<Role?> GetByIdAsync(int id);
	Task<bool> ExistsByNameAsync(string name, int? excludeId);
	Task<bool> HasAssignedClientsAsync(int id);
	Task CreateAsync(Role role);
	Task UpdateAsync(Role role);
	Task DeleteAsync(int id);
}
