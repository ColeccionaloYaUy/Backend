using ColeccionaloYa.Domain.Addresses;

namespace ColeccionaloYa.Persistence.Addresses.Interfaces;

public interface IAddressRepository {
	Task<List<Address>> GetByClientIdAsync(int clientId, CancellationToken cancellationToken);
	Task<Address?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task CreateAsync(Address address, CancellationToken cancellationToken);
	Task UpdateAsync(Address address, CancellationToken cancellationToken);
	Task LogicalDeleteAsync(int id, CancellationToken cancellationToken);
}
