using MediatR;

namespace ColeccionaloYa.Application.Roles.UpdateRole;

public record UpdateRoleCommand(int Id, string Name, string? Description) : IRequest<RoleDto>;
