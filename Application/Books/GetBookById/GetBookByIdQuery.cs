using MediatR;

namespace ColeccionaloYa.Application.Books.GetBookById;

public record GetBookByIdQuery(int Id) : IRequest<BookDetailDto>;
