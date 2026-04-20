using MediatR;

namespace ColeccionaloYa.Application.Roles.GetRoles;

public record GetRolesQuery() : IRequest<List<RoleDto>>;
