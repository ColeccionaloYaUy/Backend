using MediatR;

namespace ColeccionaloYa.Application.Books.RemoveBookGenre;

public record RemoveBookGenreCommand(int BookId, int GenreId) : IRequest<Unit>;
