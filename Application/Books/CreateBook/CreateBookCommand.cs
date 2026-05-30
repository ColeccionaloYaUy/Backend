using MediatR;

namespace ColeccionaloYa.Application.Books.CreateBook;

public record CreateBookCommand(
	string Name,
	string ShortDescription,
	string LongDescription,
	decimal Price,
	decimal Weight,
	string TypeBook,
	IReadOnlyList<int> AuthorIds,
	IReadOnlyList<int> GenreIds,
	bool IsFeatured
) : IRequest<BookSummaryDto>;
