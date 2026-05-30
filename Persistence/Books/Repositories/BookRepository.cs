using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Authors;
using ColeccionaloYa.Domain.Books;
using ColeccionaloYa.Domain.Genres;
using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Persistence.Books.Interfaces;
using ColeccionaloYa.Persistence.Books.ReadModels;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Books.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class BookRepository : IBookRepository {
	private readonly ICConnection _Connection;

	public BookRepository(ICConnection connection) {
		_Connection = connection;
	}

	public async Task<bool> ExistsAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT 1
            FROM book b
            INNER JOIN product p ON p.id_product = b.id_product
            WHERE b.id_product = @id AND p.logical_delete = FALSE";
		cmd.AddParameter("id", productId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<BookDetailData?> GetDetailAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT p.id_product, p.name, p.short_description, p.long_description, p.price,
                   p.type::text AS type, p.weight, p.is_featured, p.creation_date, p.logical_delete,
                   b.type::text AS type_book
            FROM book b
            INNER JOIN product p ON p.id_product = b.id_product
            WHERE b.id_product = @id AND p.logical_delete = FALSE";
		cmd.AddParameter("id", productId);

		var header = await cmd.ExecuteSelect(rs => {
			var product = new Product {
				Id = rs.GetValue<int>("id_product"),
				Name = rs.GetValue<string>("name"),
				ShortDescription = rs.GetValue<string>("short_description"),
				LongDescription = rs.GetValue<string>("long_description"),
				Price = rs.GetValue<decimal>("price"),
				Type = Enum.Parse<ProductType>(rs.GetValue<string>("type"), ignoreCase: true),
				Weight = rs.GetValue<decimal>("weight"),
				IsFeatured = rs.GetValue<bool>("is_featured"),
				CreationDate = rs.GetValue<DateOnly>("creation_date"),
				LogicalDelete = rs.GetValue<bool>("logical_delete"),
			};
			var book = Book.Create(product.Id, Enum.Parse<BookType>(rs.GetValue<string>("type_book"), ignoreCase: true));
			return new BookDetailData(book, product, new List<Author>(), new List<Genre>());
		}, cancellationToken);

		if (header is null) {
			return null;
		}

		var authors = await GetAuthorsAsync(productId, cancellationToken);
		var genres = await GetGenresAsync(productId, cancellationToken);
		return header with { Authors = authors, Genres = genres };
	}

	public async Task CreateAsync(Book book, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO book (id_product, type)
            VALUES (@id, @type::type_book)";
		cmd.AddParameter("id", book.ProductId);
		cmd.AddParameter("type", book.Type.ToString().ToLower());
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task UpdateTypeAsync(int productId, BookType type, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "UPDATE book SET type = @type::type_book WHERE id_product = @id";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("type", type.ToString().ToLower());
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task<List<Author>> GetAuthorsAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT a.id_author, a.name
            FROM book_author ba
            INNER JOIN author a ON a.id_author = ba.id_author
            WHERE ba.id_book = @id
            ORDER BY a.name";
		cmd.AddParameter("id", productId);
		return await cmd.ExecuteSelectList<Author>(rs => new Author {
			Id = rs.GetValue<int>("id_author"),
			Name = rs.GetValue<string>("name"),
		}, cancellationToken);
	}

	public async Task<bool> AnyAuthorLinkedAsync(int productId, IReadOnlyCollection<int> authorIds, CancellationToken cancellationToken) {
		if (authorIds.Count == 0) {
			return false;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM book_author WHERE id_book = @id AND id_author = ANY(@ids)";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("ids", authorIds.ToArray());
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> AuthorLinkExistsAsync(int productId, int authorId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM book_author WHERE id_book = @id AND id_author = @authorId";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("authorId", authorId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task AddAuthorsAsync(int productId, IReadOnlyCollection<int> authorIds, CancellationToken cancellationToken) {
		if (authorIds.Count == 0) {
			return;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO book_author (id_book, id_author)
            SELECT @id, a FROM UNNEST(@ids) AS a";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("ids", authorIds.ToArray());
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task RemoveAuthorAsync(int productId, int authorId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM book_author WHERE id_book = @id AND id_author = @authorId";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("authorId", authorId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task<List<Genre>> GetGenresAsync(int productId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT g.id_genre, g.name
            FROM book_genre bg
            INNER JOIN genre g ON g.id_genre = bg.id_genre
            WHERE bg.id_book = @id
            ORDER BY g.name";
		cmd.AddParameter("id", productId);
		return await cmd.ExecuteSelectList<Genre>(rs => new Genre {
			Id = rs.GetValue<int>("id_genre"),
			Name = rs.GetValue<string>("name"),
		}, cancellationToken);
	}

	public async Task<bool> AnyGenreLinkedAsync(int productId, IReadOnlyCollection<int> genreIds, CancellationToken cancellationToken) {
		if (genreIds.Count == 0) {
			return false;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM book_genre WHERE id_book = @id AND id_genre = ANY(@ids)";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("ids", genreIds.ToArray());
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task<bool> GenreLinkExistsAsync(int productId, int genreId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "SELECT 1 FROM book_genre WHERE id_book = @id AND id_genre = @genreId";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("genreId", genreId);
		return await cmd.ExecuteCommandExists(cancellationToken);
	}

	public async Task AddGenresAsync(int productId, IReadOnlyCollection<int> genreIds, CancellationToken cancellationToken) {
		if (genreIds.Count == 0) {
			return;
		}

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO book_genre (id_book, id_genre)
            SELECT @id, g FROM UNNEST(@ids) AS g";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("ids", genreIds.ToArray());
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task RemoveGenreAsync(int productId, int genreId, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "DELETE FROM book_genre WHERE id_book = @id AND id_genre = @genreId";
		cmd.AddParameter("id", productId);
		cmd.AddParameter("genreId", genreId);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}
}
