using MediatR;

namespace ColeccionaloYa.Application.Clients.GetClientById;

public record GetClientByIdQuery(int Id) : IRequest<ClientDto>;
