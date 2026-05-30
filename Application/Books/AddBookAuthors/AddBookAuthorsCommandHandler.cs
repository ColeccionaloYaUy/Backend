using ColeccionaloYa.Application.Authors;
using ColeccionaloYa.Domain.Authors.Exceptions;
using ColeccionaloYa.Domain.Books.Exceptions;
using ColeccionaloYa.Persistence.Authors.Interfaces;
using ColeccionaloYa.Persistence.Books.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Books.AddBookAuthors;

public class AddBookAuthorsCommandHandler : IRequestHandler<AddBookAuthorsCommand, BookAuthorsDto> {
	private readonly IBookRepository _BookRepository;
	private readonly IAuthorRepository _AuthorRepository;

	public AddBookAuthorsCommandHandler(IBookRepository bookRepository, IAuthorRepository authorRepository) {
		_BookRepository = bookRepository;
		_AuthorRepository = authorRepository;
	}

	public async Task<BookAuthorsDto> Handle(AddBookAuthorsCommand request, CancellationToken cancellationToken) {
		if (!await _BookRepository.ExistsAsync(request.BookId, cancellationToken)) {
			throw new BookNotFoundException(request.BookId);
		}

		if (!await _AuthorRepository.ExistByIdsAsync(request.AuthorIds, cancellationToken)) {
			throw new InvalidAuthorReferenceException();
		}

		if (await _BookRepository.AnyAuthorLinkedAsync(request.BookId, request.AuthorIds, cancellationToken)) {
			throw new BookAuthorAlreadyLinkedException();
		}

		await _BookRepository.AddAuthorsAsync(request.BookId, request.AuthorIds, cancellationToken);
		var authors = await _BookRepository.GetAuthorsAsync(request.BookId, cancellationToken);
		return new BookAuthorsDto(request.BookId, authors.Select(AuthorDto.From).ToList());
	}
}
