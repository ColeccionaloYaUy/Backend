using MediatR;

namespace ColeccionaloYa.Application.Genres.GetGenreById;

public record GetGenreByIdQuery(int Id) : IRequest<GenreDto>;
