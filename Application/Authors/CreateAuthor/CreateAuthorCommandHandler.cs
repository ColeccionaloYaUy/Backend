using ColeccionaloYa.Domain.Authors;
using ColeccionaloYa.Domain.Authors.Exceptions;
using ColeccionaloYa.Persistence.Authors.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Authors.CreateAuthor;

public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, AuthorDto> {
	private readonly IAuthorRepository _Repository;

	public CreateAuthorCommandHandler(IAuthorRepository repository) {
		_Repository = repository;
	}

	public async Task<AuthorDto> Handle(CreateAuthorCommand request, CancellationToken cancellationToken) {
		if (await _Repository.ExistsByNameAsync(request.Name, null, cancellationToken)) {
			throw new DuplicateAuthorNameException(request.Name);
		}

		var author = Author.Create(request.Name);
		await _Repository.CreateAsync(author, cancellationToken);
		return AuthorDto.From(author);
	}
}
