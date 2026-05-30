using ColeccionaloYa.Domain.Genres;

namespace ColeccionaloYa.Application.Genres;

public record GenreDto(int Id, string Name) {
	public static GenreDto From(Genre genre) =>
		new(genre.Id, genre.Name);
}
