using ColeccionaloYa.Domain.Clients;
using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Persistence.Clients.Interfaces;

public interface IClientRepository {
	Task<PagedData<Client>> GetPagedAsync(int page, int pageSize, string? search, int? roleId, bool? active);
	Task<Client?> GetByIdAsync(int id);
	Task<bool> ExistsByEmailAsync(string email, int? excludeId);
	Task CreateAsync(Client client);
	Task UpdateProfileAsync(Client client);
	Task UpdateByAdminAsync(Client client);
	Task UpdateActiveAsync(int id, bool active);
	Task LogicalDeleteAsync(int id);
}
