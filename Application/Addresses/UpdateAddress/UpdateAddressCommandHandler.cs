using ColeccionaloYa.Domain.Addresses.Exceptions;
using ColeccionaloYa.Persistence.Addresses.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Addresses.UpdateAddress;

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, AddressDto> {
	private readonly IAddressRepository _Repository;

	public UpdateAddressCommandHandler(IAddressRepository repository) {
		_Repository = repository;
	}

	public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken) {
		var address = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new AddressNotFoundException(request.Id);

		if (!request.IsAdmin && address.ClientId != request.RequesterId) {
			throw new AddressAccessDeniedException();
		}

		address.Update(request.Street, request.Number, request.City, request.Department, request.PostalCode, request.Type);
		await _Repository.UpdateAsync(address, cancellationToken);
		return AddressDto.From(address);
	}
}
