using MediatR;

namespace ColeccionaloYa.Application.Genres.CreateGenre;

public record CreateGenreCommand(string Name) : IRequest<GenreDto>;
