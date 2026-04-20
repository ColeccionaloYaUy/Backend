using MediatR;

namespace ColeccionaloYa.Application.Clients.ActivateClient;

public record ActivateClientCommand(int Id) : IRequest<Unit>;
