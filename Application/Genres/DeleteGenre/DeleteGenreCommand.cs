using MediatR;

namespace ColeccionaloYa.Application.Genres.DeleteGenre;

public record DeleteGenreCommand(int Id) : IRequest<Unit>;
