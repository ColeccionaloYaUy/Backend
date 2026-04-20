using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.GetClientById;

public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto> {
	private readonly IClientRepository _Repository;

	public GetClientByIdQueryHandler(IClientRepository repository) {
		_Repository = repository;
	}

	public async Task<ClientDto> Handle(GetClientByIdQuery request, CancellationToken cancellationToken) {
		var client = await _Repository.GetByIdAsync(request.Id)
			?? throw new ClientNotFoundException(request.Id);
		return ClientDto.From(client);
	}
}
