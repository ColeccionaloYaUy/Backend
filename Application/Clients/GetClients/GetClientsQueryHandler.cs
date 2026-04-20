using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Persistence.Clients.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Clients.GetClients;

public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, PagedResult<ClientDto>> {
	private readonly IClientRepository _Repository;

	public GetClientsQueryHandler(IClientRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<ClientDto>> Handle(GetClientsQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetPagedAsync(request.Page, request.PageSize, request.Search, request.RoleId, request.Active, cancellationToken);
		return data.ToPagedResult(request.PageSize, ClientDto.From);
	}
}
