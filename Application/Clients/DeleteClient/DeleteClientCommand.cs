using MediatR;

namespace ColeccionaloYa.Application.Clients.DeleteClient;

public record DeleteClientCommand(int Id) : IRequest<Unit>;
