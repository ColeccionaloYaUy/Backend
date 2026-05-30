using ColeccionaloYa.Domain.Books.Exceptions;
using ColeccionaloYa.Persistence.Books.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Books.GetBookById;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDetailDto> {
	private readonly IBookRepository _Repository;

	public GetBookByIdQueryHandler(IBookRepository repository) {
		_Repository = repository;
	}

	public async Task<BookDetailDto> Handle(GetBookByIdQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetDetailAsync(request.Id, cancellationToken)
			?? throw new BookNotFoundException(request.Id);
		return BookDetailDto.From(data);
	}
}
