using MediatR;

namespace ColeccionaloYa.Application.Roles.DeleteRole;

public record DeleteRoleCommand(int Id) : IRequest<Unit>;
