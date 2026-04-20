using ColeccionaloYa.Domain.Addresses.Exceptions;
using ColeccionaloYa.Persistence.Addresses.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Addresses.DeleteAddress;

public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, Unit> {
	private readonly IAddressRepository _Repository;

	public DeleteAddressCommandHandler(IAddressRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteAddressCommand request, CancellationToken cancellationToken) {
		var address = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new AddressNotFoundException(request.Id);

		if (!request.IsAdmin && address.ClientId != request.RequesterId) {
			throw new AddressAccessDeniedException();
		}

		address.MarkDeleted();
		await _Repository.LogicalDeleteAsync(address.Id, cancellationToken);
		return Unit.Value;
	}
}
