using ColeccionaloYa.Domain.Addresses.Exceptions;
using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Addresses.Interfaces;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Addresses.GetAddressesByClient;

public class GetAddressesByClientQueryHandler : IRequestHandler<GetAddressesByClientQuery, List<AddressDto>> {
	private readonly IAddressRepository _AddressRepository;
	private readonly IClientRepository _ClientRepository;

	public GetAddressesByClientQueryHandler(IAddressRepository addressRepository, IClientRepository clientRepository) {
		_AddressRepository = addressRepository;
		_ClientRepository = clientRepository;
	}

	public async Task<List<AddressDto>> Handle(GetAddressesByClientQuery request, CancellationToken cancellationToken) {
		if (!request.IsAdmin && request.ClientId != request.RequesterId) {
			throw new AddressAccessDeniedException();
		}

		var client = await _ClientRepository.GetByIdAsync(request.ClientId, cancellationToken)
			?? throw new ClientNotFoundException(request.ClientId);

		var addresses = await _AddressRepository.GetByClientIdAsync(client.Id, cancellationToken);
		return addresses.Select(AddressDto.From).ToList();
	}
}
