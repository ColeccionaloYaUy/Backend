using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.UpdateMyProfile;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, ClientDto> {
	private readonly IClientRepository _Repository;

	public UpdateMyProfileCommandHandler(IClientRepository repository) {
		_Repository = repository;
	}

	public async Task<ClientDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken) {
		var client = await _Repository.GetByIdAsync(request.ClientId, cancellationToken)
			?? throw new ClientNotFoundException(request.ClientId);

		client.UpdateProfile(request.Name, request.Lastname, request.Phone);
		await _Repository.UpdateProfileAsync(client, cancellationToken);
		return ClientDto.From(client);
	}
}
