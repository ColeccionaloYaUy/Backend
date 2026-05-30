using ColeccionaloYa.Application.Genres;
using ColeccionaloYa.Domain.Books.Exceptions;
using ColeccionaloYa.Domain.Genres.Exceptions;
using ColeccionaloYa.Persistence.Books.Interfaces;
using ColeccionaloYa.Persistence.Genres.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Books.AddBookGenres;

public class AddBookGenresCommandHandler : IRequestHandler<AddBookGenresCommand, BookGenresDto> {
	private readonly IBookRepository _BookRepository;
	private readonly IGenreRepository _GenreRepository;

	public AddBookGenresCommandHandler(IBookRepository bookRepository, IGenreRepository genreRepository) {
		_BookRepository = bookRepository;
		_GenreRepository = genreRepository;
	}

	public async Task<BookGenresDto> Handle(AddBookGenresCommand request, CancellationToken cancellationToken) {
		if (!await _BookRepository.ExistsAsync(request.BookId, cancellationToken)) {
			throw new BookNotFoundException(request.BookId);
		}

		if (!await _GenreRepository.ExistByIdsAsync(request.GenreIds, cancellationToken)) {
			throw new InvalidGenreReferenceException();
		}

		if (await _BookRepository.AnyGenreLinkedAsync(request.BookId, request.GenreIds, cancellationToken)) {
			throw new BookGenreAlreadyLinkedException();
		}

		await _BookRepository.AddGenresAsync(request.BookId, request.GenreIds, cancellationToken);
		var genres = await _BookRepository.GetGenresAsync(request.BookId, cancellationToken);
		return new BookGenresDto(request.BookId, genres.Select(GenreDto.From).ToList());
	}
}
