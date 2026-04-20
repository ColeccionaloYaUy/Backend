using ColeccionaloYa.Persistence.Roles.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Roles.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>> {
	private readonly IRoleRepository _Repository;

	public GetRolesQueryHandler(IRoleRepository repository) {
		_Repository = repository;
	}

	public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken) {
		var roles = await _Repository.GetAllAsync(cancellationToken);
		return roles.Select(RoleDto.From).ToList();
	}
}
