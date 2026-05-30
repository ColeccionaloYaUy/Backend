using ColeccionaloYa.Domain.Authors.Exceptions;
using ColeccionaloYa.Persistence.Authors.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Authors.GetAuthorById;

public class GetAuthorByIdQueryHandler : IRequestHandler<GetAuthorByIdQuery, AuthorDto> {
	private readonly IAuthorRepository _Repository;

	public GetAuthorByIdQueryHandler(IAuthorRepository repository) {
		_Repository = repository;
	}

	public async Task<AuthorDto> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken) {
		var author = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new AuthorNotFoundException(request.Id);
		return AuthorDto.From(author);
	}
}
