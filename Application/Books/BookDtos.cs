using ColeccionaloYa.Application.Authors;
using ColeccionaloYa.Application.Genres;
using ColeccionaloYa.Application.Products;
using ColeccionaloYa.Persistence.Books.ReadModels;

namespace ColeccionaloYa.Application.Books;

public record BookInfoDto(int IdProduct, string TypeBook);

public record BookDetailDto(
	BookInfoDto Book,
	ProductDto Product,
	List<AuthorDto> Authors,
	List<GenreDto> Genres
) {
	public static BookDetailDto From(BookDetailData data) =>
		new(
			new BookInfoDto(data.Book.ProductId, data.Book.Type.ToString().ToLowerInvariant()),
			ProductDto.From(data.Product),
			data.Authors.Select(AuthorDto.From).ToList(),
			data.Genres.Select(GenreDto.From).ToList());
}

public record BookSummaryDto(
	int IdProduct,
	string Name,
	string TypeBook,
	List<AuthorDto> Authors,
	List<GenreDto> Genres
) {
	public static BookSummaryDto From(BookDetailData data) =>
		new(
			data.Product.Id,
			data.Product.Name,
			data.Book.Type.ToString().ToLowerInvariant(),
			data.Authors.Select(AuthorDto.From).ToList(),
			data.Genres.Select(GenreDto.From).ToList());
}

public record BookAuthorsDto(int IdProduct, List<AuthorDto> Authors);

public record BookGenresDto(int IdProduct, List<GenreDto> Genres);
