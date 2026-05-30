using MediatR;

namespace ColeccionaloYa.Application.Genres.GetGenres;

public record GetGenresQuery() : IRequest<List<GenreDto>>;
