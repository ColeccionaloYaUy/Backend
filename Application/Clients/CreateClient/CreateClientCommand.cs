using MediatR;

namespace ColeccionaloYa.Application.Clients.CreateClient;

public record CreateClientCommand(
	string Name,
	string Lastname,
	string Email,
	string? Phone,
	string Password,
	int RoleId,
	bool Active
) : IRequest<ClientDto>;
