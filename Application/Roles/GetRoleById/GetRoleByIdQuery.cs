using MediatR;

namespace ColeccionaloYa.Application.Roles.GetRoleById;

public record GetRoleByIdQuery(int Id) : IRequest<RoleDto>;
