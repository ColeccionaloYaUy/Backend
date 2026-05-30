using ColeccionaloYa.Domain.Books.Exceptions;
using ColeccionaloYa.Persistence.Books.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Books.RemoveBookGenre;

public class RemoveBookGenreCommandHandler : IRequestHandler<RemoveBookGenreCommand, Unit> {
	private readonly IBookRepository _BookRepository;

	public RemoveBookGenreCommandHandler(IBookRepository bookRepository) {
		_BookRepository = bookRepository;
	}

	public async Task<Unit> Handle(RemoveBookGenreCommand request, CancellationToken cancellationToken) {
		if (!await _BookRepository.GenreLinkExistsAsync(request.BookId, request.GenreId, cancellationToken)) {
			throw new BookGenreNotLinkedException(request.BookId, request.GenreId);
		}

		await _BookRepository.RemoveGenreAsync(request.BookId, request.GenreId, cancellationToken);
		return Unit.Value;
	}
}
