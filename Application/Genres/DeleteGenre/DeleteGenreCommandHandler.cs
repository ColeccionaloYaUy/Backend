using ColeccionaloYa.Domain.Genres.Exceptions;
using ColeccionaloYa.Persistence.Genres.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Genres.DeleteGenre;

public class DeleteGenreCommandHandler : IRequestHandler<DeleteGenreCommand, Unit> {
	private readonly IGenreRepository _Repository;

	public DeleteGenreCommandHandler(IGenreRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteGenreCommand request, CancellationToken cancellationToken) {
		var genre = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new GenreNotFoundException(request.Id);

		if (await _Repository.HasBooksAsync(request.Id, cancellationToken)) {
			throw new GenreInUseException(request.Id);
		}

		await _Repository.DeleteAsync(genre.Id, cancellationToken);
		return Unit.Value;
	}
}
