using ColeccionaloYa.Domain.Roles.Exceptions;
using ColeccionaloYa.Persistence.Roles.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Roles.GetRoleById;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto> {
	private readonly IRoleRepository _Repository;

	public GetRoleByIdQueryHandler(IRoleRepository repository) {
		_Repository = repository;
	}

	public async Task<RoleDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken) {
		var role = await _Repository.GetByIdAsync(request.Id)
			?? throw new RoleNotFoundException(request.Id);
		return RoleDto.From(role);
	}
}
