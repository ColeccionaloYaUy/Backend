using ColeccionaloYa.Domain.Authors;
using ColeccionaloYa.Domain.Books;
using ColeccionaloYa.Domain.Genres;
using ColeccionaloYa.Persistence.Books.ReadModels;

namespace ColeccionaloYa.Persistence.Books.Interfaces;

public interface IBookRepository {
	Task<bool> ExistsAsync(int productId, CancellationToken cancellationToken);
	Task<BookDetailData?> GetDetailAsync(int productId, CancellationToken cancellationToken);
	Task CreateAsync(Book book, CancellationToken cancellationToken);
	Task UpdateTypeAsync(int productId, BookType type, CancellationToken cancellationToken);

	Task<List<Author>> GetAuthorsAsync(int productId, CancellationToken cancellationToken);
	Task<bool> AnyAuthorLinkedAsync(int productId, IReadOnlyCollection<int> authorIds, CancellationToken cancellationToken);
	Task<bool> AuthorLinkExistsAsync(int productId, int authorId, CancellationToken cancellationToken);
	Task AddAuthorsAsync(int productId, IReadOnlyCollection<int> authorIds, CancellationToken cancellationToken);
	Task RemoveAuthorAsync(int productId, int authorId, CancellationToken cancellationToken);

	Task<List<Genre>> GetGenresAsync(int productId, CancellationToken cancellationToken);
	Task<bool> AnyGenreLinkedAsync(int productId, IReadOnlyCollection<int> genreIds, CancellationToken cancellationToken);
	Task<bool> GenreLinkExistsAsync(int productId, int genreId, CancellationToken cancellationToken);
	Task AddGenresAsync(int productId, IReadOnlyCollection<int> genreIds, CancellationToken cancellationToken);
	Task RemoveGenreAsync(int productId, int genreId, CancellationToken cancellationToken);
}
