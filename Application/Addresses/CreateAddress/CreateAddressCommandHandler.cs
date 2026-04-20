using ColeccionaloYa.Domain.Addresses;
using ColeccionaloYa.Domain.Addresses.Exceptions;
using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Addresses.Interfaces;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Addresses.CreateAddress;

public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto> {
	private readonly IAddressRepository _AddressRepository;
	private readonly IClientRepository _ClientRepository;

	public CreateAddressCommandHandler(IAddressRepository addressRepository, IClientRepository clientRepository) {
		_AddressRepository = addressRepository;
		_ClientRepository = clientRepository;
	}

	public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken) {
		if (!request.IsAdmin && request.ClientId != request.RequesterId) {
			throw new AddressAccessDeniedException();
		}

		var client = await _ClientRepository.GetByIdAsync(request.ClientId)
			?? throw new ClientNotFoundException(request.ClientId);

		var address = Address.Create(
			client.Id,
			request.Street,
			request.Number,
			request.City,
			request.Department,
			request.PostalCode,
			request.Type
		);

		await _AddressRepository.CreateAsync(address);
		return AddressDto.From(address);
	}
}
