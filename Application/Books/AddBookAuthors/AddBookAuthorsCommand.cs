using MediatR;

namespace ColeccionaloYa.Application.Books.AddBookAuthors;

public record AddBookAuthorsCommand(int BookId, IReadOnlyList<int> AuthorIds) : IRequest<BookAuthorsDto>;
