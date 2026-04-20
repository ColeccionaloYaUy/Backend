using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Clients.GetClients;

public record GetClientsQuery(
	int Page,
	int PageSize,
	string? Search,
	int? RoleId,
	bool? Active
) : IRequest<PagedResult<ClientDto>>, IPagedQuery;
