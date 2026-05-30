using ColeccionaloYa.Domain.Packs.Exceptions;
using ColeccionaloYa.Persistence.Packs.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Packs.GetPackById;

public class GetPackByIdQueryHandler : IRequestHandler<GetPackByIdQuery, PackDetailDto> {
	private readonly IPackRepository _Repository;

	public GetPackByIdQueryHandler(IPackRepository repository) {
		_Repository = repository;
	}

	public async Task<PackDetailDto> Handle(GetPackByIdQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetDetailAsync(request.Id, cancellationToken)
			?? throw new PackNotFoundException(request.Id);
		return PackDetailDto.From(data);
	}
}
