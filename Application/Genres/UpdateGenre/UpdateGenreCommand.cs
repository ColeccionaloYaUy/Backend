using MediatR;

namespace ColeccionaloYa.Application.Genres.UpdateGenre;

public record UpdateGenreCommand(int Id, string Name) : IRequest<GenreDto>;
