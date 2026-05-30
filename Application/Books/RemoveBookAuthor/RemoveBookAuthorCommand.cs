using MediatR;

namespace ColeccionaloYa.Application.Books.RemoveBookAuthor;

public record RemoveBookAuthorCommand(int BookId, int AuthorId) : IRequest<Unit>;
