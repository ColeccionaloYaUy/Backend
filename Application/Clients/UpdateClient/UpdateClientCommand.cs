using MediatR;

namespace ColeccionaloYa.Application.Clients.UpdateClient;

public record UpdateClientCommand(
	int Id,
	string Name,
	string Lastname,
	string? Phone,
	int RoleId,
	bool Active
) : IRequest<ClientDto>;
