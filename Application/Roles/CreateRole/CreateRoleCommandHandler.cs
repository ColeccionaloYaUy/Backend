using ColeccionaloYa.Domain.Roles;
using ColeccionaloYa.Domain.Roles.Exceptions;
using ColeccionaloYa.Persistence.Roles.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Roles.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto> {
	private readonly IRoleRepository _Repository;

	public CreateRoleCommandHandler(IRoleRepository repository) {
		_Repository = repository;
	}

	public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken) {
		if (await _Repository.ExistsByNameAsync(request.Name, null)) {
			throw new DuplicateRoleNameException(request.Name);
		}

		var role = Role.Create(request.Name, request.Description);
		await _Repository.CreateAsync(role);
		return RoleDto.From(role);
	}
}
