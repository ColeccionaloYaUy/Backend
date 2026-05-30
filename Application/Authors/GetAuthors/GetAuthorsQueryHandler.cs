using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Persistence.Authors.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Authors.GetAuthors;

public class GetAuthorsQueryHandler : IRequestHandler<GetAuthorsQuery, PagedResult<AuthorDto>> {
	private readonly IAuthorRepository _Repository;

	public GetAuthorsQueryHandler(IAuthorRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<AuthorDto>> Handle(GetAuthorsQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
		return data.ToPagedResult(request.PageSize, AuthorDto.From);
	}
}
