using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.GetMyProfile;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ClientDto> {
	private readonly IClientRepository _Repository;

	public GetMyProfileQueryHandler(IClientRepository repository) {
		_Repository = repository;
	}

	public async Task<ClientDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken) {
		var client = await _Repository.GetByIdAsync(request.ClientId, cancellationToken)
			?? throw new ClientNotFoundException(request.ClientId);
		return ClientDto.From(client);
	}
}
