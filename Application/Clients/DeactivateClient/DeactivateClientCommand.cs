using MediatR;

namespace ColeccionaloYa.Application.Clients.DeactivateClient;

public record DeactivateClientCommand(int Id) : IRequest<Unit>;
