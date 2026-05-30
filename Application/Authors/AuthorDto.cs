using ColeccionaloYa.Domain.Authors;

namespace ColeccionaloYa.Application.Authors;

public record AuthorDto(int Id, string Name) {
	public static AuthorDto From(Author author) =>
		new(author.Id, author.Name);
}
