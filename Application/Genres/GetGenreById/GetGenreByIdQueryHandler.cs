using ColeccionaloYa.Domain.Genres.Exceptions;
using ColeccionaloYa.Persistence.Genres.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Genres.GetGenreById;

public class GetGenreByIdQueryHandler : IRequestHandler<GetGenreByIdQuery, GenreDto> {
	private readonly IGenreRepository _Repository;

	public GetGenreByIdQueryHandler(IGenreRepository repository) {
		_Repository = repository;
	}

	public async Task<GenreDto> Handle(GetGenreByIdQuery request, CancellationToken cancellationToken) {
		var genre = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new GenreNotFoundException(request.Id);
		return GenreDto.From(genre);
	}
}
