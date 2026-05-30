using ColeccionaloYa.Persistence.Genres.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Genres.GetGenres;

public class GetGenresQueryHandler : IRequestHandler<GetGenresQuery, List<GenreDto>> {
	private readonly IGenreRepository _Repository;

	public GetGenresQueryHandler(IGenreRepository repository) {
		_Repository = repository;
	}

	public async Task<List<GenreDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken) {
		var genres = await _Repository.GetAllAsync(cancellationToken);
		return genres.Select(GenreDto.From).ToList();
	}
}
