using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Domain.Roles.Exceptions;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using ColeccionaloYa.Persistence.Roles.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.UpdateClient;

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientDto> {
	private readonly IClientRepository _ClientRepository;
	private readonly IRoleRepository _RoleRepository;

	public UpdateClientCommandHandler(IClientRepository clientRepository, IRoleRepository roleRepository) {
		_ClientRepository = clientRepository;
		_RoleRepository = roleRepository;
	}

	public async Task<ClientDto> Handle(UpdateClientCommand request, CancellationToken cancellationToken) {
		var client = await _ClientRepository.GetByIdAsync(request.Id)
			?? throw new ClientNotFoundException(request.Id);

		var role = await _RoleRepository.GetByIdAsync(request.RoleId)
			?? throw new RoleNotFoundException(request.RoleId);

		client.UpdateByAdmin(request.Name, request.Lastname, request.Phone, role.Id, role.Name, request.Active);
		await _ClientRepository.UpdateByAdminAsync(client);
		return ClientDto.From(client);
	}
}
