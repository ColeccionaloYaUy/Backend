using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Books.Exceptions;
using ColeccionaloYa.Persistence.Books.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Books.UpdateBook;

public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, BookDetailDto> {
	private readonly ICConnection _Connection;
	private readonly IProductRepository _ProductRepository;
	private readonly IBookRepository _BookRepository;

	public UpdateBookCommandHandler(
		ICConnection connection,
		IProductRepository productRepository,
		IBookRepository bookRepository) {
		_Connection = connection;
		_ProductRepository = productRepository;
		_BookRepository = bookRepository;
	}

	public async Task<BookDetailDto> Handle(UpdateBookCommand request, CancellationToken cancellationToken) {
		if (!await _BookRepository.ExistsAsync(request.Id, cancellationToken)) {
			throw new BookNotFoundException(request.Id);
		}

		var product = await _ProductRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new BookNotFoundException(request.Id);

		var bookType = BookTypeParser.ParseOptional(request.TypeBook);

		// Type stays 'book'; only the catalog fields are mutated here.
		product.Update(request.Name, request.ShortDescription, request.LongDescription,
			request.Price, null, request.Weight, null);

		await _Connection.BeginTransaction(cancellationToken);
		try {
			await _ProductRepository.UpdateAsync(product, cancellationToken);
			if (bookType.HasValue) {
				await _BookRepository.UpdateTypeAsync(request.Id, bookType.Value, cancellationToken);
			}
			await _Connection.CommitTransaction(cancellationToken);
		} catch {
			await _Connection.CancelTransaction(cancellationToken);
			throw;
		}

		var data = await _BookRepository.GetDetailAsync(request.Id, cancellationToken)
			?? throw new BookNotFoundException(request.Id);
		return BookDetailDto.From(data);
	}
}
