using MediatR;

namespace ColeccionaloYa.Application.Roles.CreateRole;

public record CreateRoleCommand(string Name, string? Description) : IRequest<RoleDto>;
