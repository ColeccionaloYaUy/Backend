using ColeccionaloYa.Domain.Authors.Exceptions;
using ColeccionaloYa.Persistence.Authors.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Authors.DeleteAuthor;

public class DeleteAuthorCommandHandler : IRequestHandler<DeleteAuthorCommand, Unit> {
	private readonly IAuthorRepository _Repository;

	public DeleteAuthorCommandHandler(IAuthorRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteAuthorCommand request, CancellationToken cancellationToken) {
		var author = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new AuthorNotFoundException(request.Id);

		if (await _Repository.HasBooksAsync(request.Id, cancellationToken)) {
			throw new AuthorInUseException(request.Id);
		}

		await _Repository.DeleteAsync(author.Id, cancellationToken);
		return Unit.Value;
	}
}
