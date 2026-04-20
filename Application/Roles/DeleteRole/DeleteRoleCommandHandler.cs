using ColeccionaloYa.Domain.Roles.Exceptions;
using ColeccionaloYa.Persistence.Roles.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Roles.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Unit> {
	private readonly IRoleRepository _Repository;

	public DeleteRoleCommandHandler(IRoleRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken) {
		var role = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new RoleNotFoundException(request.Id);

		if (await _Repository.HasAssignedClientsAsync(request.Id, cancellationToken)) {
			throw new RoleInUseException(request.Id);
		}

		await _Repository.DeleteAsync(role.Id, cancellationToken);
		return Unit.Value;
	}
}
