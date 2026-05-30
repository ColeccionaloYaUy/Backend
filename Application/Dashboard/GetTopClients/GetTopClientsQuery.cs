using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetTopClients;

public record GetTopClientsQuery(int Limit, DateOnly? DateFrom, DateOnly? DateTo) : IRequest<TopClientsDto>;
