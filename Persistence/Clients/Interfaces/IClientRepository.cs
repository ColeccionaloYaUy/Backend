using ColeccionaloYa.Domain.Clients;
using ColeccionaloYa.Persistence.Shared;

namespace ColeccionaloYa.Persistence.Clients.Interfaces;

public interface IClientRepository {
	Task<PagedData<Client>> GetPagedAsync(int page, int pageSize, string? search, int? roleId, bool? active, CancellationToken cancellationToken);
	Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<bool> ExistsByEmailAsync(string email, int? excludeId, CancellationToken cancellationToken);
	Task CreateAsync(Client client, CancellationToken cancellationToken);
	Task UpdateProfileAsync(Client client, CancellationToken cancellationToken);
	Task UpdateByAdminAsync(Client client, CancellationToken cancellationToken);
	Task UpdateActiveAsync(int id, bool active, CancellationToken cancellationToken);
	Task LogicalDeleteAsync(int id, CancellationToken cancellationToken);
}
