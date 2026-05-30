using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Authors.Exceptions;
using ColeccionaloYa.Domain.Books;
using ColeccionaloYa.Domain.Genres.Exceptions;
using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Persistence.Authors.Interfaces;
using ColeccionaloYa.Persistence.Books.Interfaces;
using ColeccionaloYa.Persistence.Books.ReadModels;
using ColeccionaloYa.Persistence.Genres.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Books.CreateBook;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, BookSummaryDto> {
	private readonly ICConnection _Connection;
	private readonly IProductRepository _ProductRepository;
	private readonly IBookRepository _BookRepository;
	private readonly IAuthorRepository _AuthorRepository;
	private readonly IGenreRepository _GenreRepository;

	public CreateBookCommandHandler(
		ICConnection connection,
		IProductRepository productRepository,
		IBookRepository bookRepository,
		IAuthorRepository authorRepository,
		IGenreRepository genreRepository) {
		_Connection = connection;
		_ProductRepository = productRepository;
		_BookRepository = bookRepository;
		_AuthorRepository = authorRepository;
		_GenreRepository = genreRepository;
	}

	public async Task<BookSummaryDto> Handle(CreateBookCommand request, CancellationToken cancellationToken) {
		var bookType = BookTypeParser.Parse(request.TypeBook);

		if (!await _AuthorRepository.ExistByIdsAsync(request.AuthorIds, cancellationToken)) {
			throw new InvalidAuthorReferenceException();
		}

		if (!await _GenreRepository.ExistByIdsAsync(request.GenreIds, cancellationToken)) {
			throw new InvalidGenreReferenceException();
		}

		var product = Product.Create(
			request.Name, request.ShortDescription, request.LongDescription,
			request.Price, ProductType.Book, request.Weight, request.IsFeatured);

		await _Connection.BeginTransaction(cancellationToken);
		try {
			await _ProductRepository.CreateAsync(product, cancellationToken);

			var book = Book.Create(product.Id, bookType);
			await _BookRepository.CreateAsync(book, cancellationToken);

			await _BookRepository.AddAuthorsAsync(product.Id, request.AuthorIds, cancellationToken);
			await _BookRepository.AddGenresAsync(product.Id, request.GenreIds, cancellationToken);

			await _Connection.CommitTransaction(cancellationToken);

			var authors = await _BookRepository.GetAuthorsAsync(product.Id, cancellationToken);
			var genres = await _BookRepository.GetGenresAsync(product.Id, cancellationToken);
			return BookSummaryDto.From(new BookDetailData(book, product, authors, genres));
		} catch {
			await _Connection.CancelTransaction(cancellationToken);
			throw;
		}
	}
}
