using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.DeleteClient;

public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Unit> {
	private readonly IClientRepository _Repository;

	public DeleteClientCommandHandler(IClientRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteClientCommand request, CancellationToken cancellationToken) {
		var client = await _Repository.GetByIdAsync(request.Id)
			?? throw new ClientNotFoundException(request.Id);

		client.MarkDeleted();
		await _Repository.LogicalDeleteAsync(client.Id);
		return Unit.Value;
	}
}
