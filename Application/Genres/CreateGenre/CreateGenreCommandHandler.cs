using ColeccionaloYa.Domain.Genres;
using ColeccionaloYa.Domain.Genres.Exceptions;
using ColeccionaloYa.Persistence.Genres.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Genres.CreateGenre;

public class CreateGenreCommandHandler : IRequestHandler<CreateGenreCommand, GenreDto> {
	private readonly IGenreRepository _Repository;

	public CreateGenreCommandHandler(IGenreRepository repository) {
		_Repository = repository;
	}

	public async Task<GenreDto> Handle(CreateGenreCommand request, CancellationToken cancellationToken) {
		if (await _Repository.ExistsByNameAsync(request.Name, null, cancellationToken)) {
			throw new DuplicateGenreNameException(request.Name);
		}

		var genre = Genre.Create(request.Name);
		await _Repository.CreateAsync(genre, cancellationToken);
		return GenreDto.From(genre);
	}
}
