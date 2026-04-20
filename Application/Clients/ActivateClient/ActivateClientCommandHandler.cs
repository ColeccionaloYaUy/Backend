using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.ActivateClient;

public class ActivateClientCommandHandler : IRequestHandler<ActivateClientCommand, Unit> {
	private readonly IClientRepository _Repository;

	public ActivateClientCommandHandler(IClientRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(ActivateClientCommand request, CancellationToken cancellationToken) {
		var client = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new ClientNotFoundException(request.Id);

		client.Activate();
		await _Repository.UpdateActiveAsync(client.Id, client.Active, cancellationToken);
		return Unit.Value;
	}
}
