using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.DeactivateClient;

public class DeactivateClientCommandHandler : IRequestHandler<DeactivateClientCommand, Unit> {
	private readonly IClientRepository _Repository;

	public DeactivateClientCommandHandler(IClientRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeactivateClientCommand request, CancellationToken cancellationToken) {
		var client = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new ClientNotFoundException(request.Id);

		client.Deactivate();
		await _Repository.UpdateActiveAsync(client.Id, client.Active, cancellationToken);
		return Unit.Value;
	}
}
