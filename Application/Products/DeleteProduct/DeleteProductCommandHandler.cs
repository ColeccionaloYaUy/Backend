using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Products.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit> {
	private readonly IProductRepository _Repository;

	public DeleteProductCommandHandler(IProductRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken) {
		var product = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new ProductNotFoundException(request.Id);

		if (await _Repository.IsInUseAsync(request.Id, cancellationToken)) {
			throw new ProductInUseException(request.Id);
		}

		await _Repository.LogicalDeleteAsync(product.Id, cancellationToken);
		return Unit.Value;
	}
}
