using ColeccionaloYa.Domain.Roles.Exceptions;
using ColeccionaloYa.Persistence.Roles.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Roles.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto> {
	private readonly IRoleRepository _Repository;

	public UpdateRoleCommandHandler(IRoleRepository repository) {
		_Repository = repository;
	}

	public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken) {
		var role = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new RoleNotFoundException(request.Id);

		if (await _Repository.ExistsByNameAsync(request.Name, request.Id, cancellationToken)) {
			throw new DuplicateRoleNameException(request.Name);
		}

		role.Update(request.Name, request.Description);
		await _Repository.UpdateAsync(role, cancellationToken);
		return RoleDto.From(role);
	}
}
