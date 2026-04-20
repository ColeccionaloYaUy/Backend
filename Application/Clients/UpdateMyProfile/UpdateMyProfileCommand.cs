using MediatR;

namespace ColeccionaloYa.Application.Clients.UpdateMyProfile;

public record UpdateMyProfileCommand(
	int ClientId,
	string Name,
	string Lastname,
	string? Phone
) : IRequest<ClientDto>;
