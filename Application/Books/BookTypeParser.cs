using ColeccionaloYa.Domain.Books;
using ColeccionaloYa.Domain.Books.Exceptions;

namespace ColeccionaloYa.Application.Books;

public static class BookTypeParser {
	public static BookType Parse(string type) {
		if (!Enum.TryParse<BookType>(type, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed)) {
			throw new InvalidBookTypeException(type);
		}
		return parsed;
	}

	public static BookType? ParseOptional(string? type) {
		return type is null ? null : Parse(type);
	}
}
