using MediatR;

namespace ColeccionaloYa.Application.Books.UpdateBook;

public record UpdateBookCommand(
	int Id,
	string? Name,
	string? ShortDescription,
	string? LongDescription,
	decimal? Price,
	decimal? Weight,
	string? TypeBook
) : IRequest<BookDetailDto>;
