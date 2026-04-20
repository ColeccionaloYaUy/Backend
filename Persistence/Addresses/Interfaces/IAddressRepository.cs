using ColeccionaloYa.Domain.Addresses;

namespace ColeccionaloYa.Persistence.Addresses.Interfaces;

public interface IAddressRepository {
	Task<List<Address>> GetByClientIdAsync(int clientId);
	Task<Address?> GetByIdAsync(int id);
	Task CreateAsync(Address address);
	Task UpdateAsync(Address address);
	Task LogicalDeleteAsync(int id);
}
