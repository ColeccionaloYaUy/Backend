using MediatR;

namespace ColeccionaloYa.Application.Clients.GetMyProfile;

public record GetMyProfileQuery(int ClientId) : IRequest<ClientDto>;
