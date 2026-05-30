using MediatR;

namespace ColeccionaloYa.Application.Books.AddBookGenres;

public record AddBookGenresCommand(int BookId, IReadOnlyList<int> GenreIds) : IRequest<BookGenresDto>;
