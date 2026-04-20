using ColeccionaloYa.Domain.Clients;
using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Domain.Roles.Exceptions;
using ColeccionaloYa.Persistence.Auth.Interfaces;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using ColeccionaloYa.Persistence.Roles.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.CreateClient;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto> {
	private readonly IClientRepository _ClientRepository;
	private readonly IRoleRepository _RoleRepository;
	private readonly IPasswordHasher _PasswordHasher;

	public CreateClientCommandHandler(
		IClientRepository clientRepository,
		IRoleRepository roleRepository,
		IPasswordHasher passwordHasher
	) {
		_ClientRepository = clientRepository;
		_RoleRepository = roleRepository;
		_PasswordHasher = passwordHasher;
	}

	public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken) {
		if (await _ClientRepository.ExistsByEmailAsync(request.Email, null)) {
			throw new EmailAlreadyRegisteredException();
		}

		var role = await _RoleRepository.GetByIdAsync(request.RoleId)
			?? throw new RoleNotFoundException(request.RoleId);

		var hash = _PasswordHasher.Hash(request.Password);
		var client = Client.CreateByAdmin(
			request.Name,
			request.Lastname,
			request.Email,
			request.Phone,
			hash,
			role.Id,
			role.Name,
			request.Active
		);

		await _ClientRepository.CreateAsync(client);
		return ClientDto.From(client);
	}
}
