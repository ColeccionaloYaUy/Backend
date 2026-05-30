using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Authors.GetAuthors;

public record GetAuthorsQuery(
	int Page,
	int PageSize,
	string? Search
) : IRequest<PagedResult<AuthorDto>>, IPagedQuery;
