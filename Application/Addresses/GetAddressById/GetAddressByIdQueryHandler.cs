using ColeccionaloYa.Domain.Addresses.Exceptions;
using ColeccionaloYa.Persistence.Addresses.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Addresses.GetAddressById;

public class GetAddressByIdQueryHandler : IRequestHandler<GetAddressByIdQuery, AddressDto> {
	private readonly IAddressRepository _Repository;

	public GetAddressByIdQueryHandler(IAddressRepository repository) {
		_Repository = repository;
	}

	public async Task<AddressDto> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken) {
		var address = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new AddressNotFoundException(request.Id);

		if (!request.IsAdmin && address.ClientId != request.RequesterId) {
			throw new AddressAccessDeniedException();
		}

		return AddressDto.From(address);
	}
}
