using ColeccionaloYa.Domain.Authors;
using ColeccionaloYa.Domain.Books;
using ColeccionaloYa.Domain.Genres;
using ColeccionaloYa.Domain.Products;

namespace ColeccionaloYa.Persistence.Books.ReadModels;

public record BookDetailData(
	Book Book,
	Product Product,
	List<Author> Authors,
	List<Genre> Genres
);
