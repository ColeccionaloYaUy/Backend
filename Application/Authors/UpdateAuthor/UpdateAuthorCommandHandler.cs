using ColeccionaloYa.Domain.Authors.Exceptions;
using ColeccionaloYa.Persistence.Authors.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Authors.UpdateAuthor;

public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, AuthorDto> {
	private readonly IAuthorRepository _Repository;

	public UpdateAuthorCommandHandler(IAuthorRepository repository) {
		_Repository = repository;
	}

	public async Task<AuthorDto> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken) {
		var author = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new AuthorNotFoundException(request.Id);

		if (await _Repository.ExistsByNameAsync(request.Name, request.Id, cancellationToken)) {
			throw new DuplicateAuthorNameException(request.Name);
		}

		author.Update(request.Name);
		await _Repository.UpdateAsync(author, cancellationToken);
		return AuthorDto.From(author);
	}
}
