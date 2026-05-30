using ColeccionaloYa.Domain.Books.Exceptions;
using ColeccionaloYa.Persistence.Books.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Books.RemoveBookAuthor;

public class RemoveBookAuthorCommandHandler : IRequestHandler<RemoveBookAuthorCommand, Unit> {
	private readonly IBookRepository _BookRepository;

	public RemoveBookAuthorCommandHandler(IBookRepository bookRepository) {
		_BookRepository = bookRepository;
	}

	public async Task<Unit> Handle(RemoveBookAuthorCommand request, CancellationToken cancellationToken) {
		if (!await _BookRepository.AuthorLinkExistsAsync(request.BookId, request.AuthorId, cancellationToken)) {
			throw new BookAuthorNotLinkedException(request.BookId, request.AuthorId);
		}

		await _BookRepository.RemoveAuthorAsync(request.BookId, request.AuthorId, cancellationToken);
		return Unit.Value;
	}
}
