using ColeccionaloYa.Domain.Genres.Exceptions;
using ColeccionaloYa.Persistence.Genres.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Genres.UpdateGenre;

public class UpdateGenreCommandHandler : IRequestHandler<UpdateGenreCommand, GenreDto> {
	private readonly IGenreRepository _Repository;

	public UpdateGenreCommandHandler(IGenreRepository repository) {
		_Repository = repository;
	}

	public async Task<GenreDto> Handle(UpdateGenreCommand request, CancellationToken cancellationToken) {
		var genre = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new GenreNotFoundException(request.Id);

		if (await _Repository.ExistsByNameAsync(request.Name, request.Id, cancellationToken)) {
			throw new DuplicateGenreNameException(request.Name);
		}

		genre.Update(request.Name);
		await _Repository.UpdateAsync(genre, cancellationToken);
		return GenreDto.From(genre);
	}
}
