using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Products.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto> {
	private readonly IProductRepository _Repository;

	public UpdateProductCommandHandler(IProductRepository repository) {
		_Repository = repository;
	}

	public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken) {
		var product = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new ProductNotFoundException(request.Id);

		var type = ProductTypeParser.ParseOptional(request.Type);

		product.Update(
			request.Name,
			request.ShortDescription,
			request.LongDescription,
			request.Price,
			type,
			request.Weight,
			request.IsFeatured);

		await _Repository.UpdateAsync(product, cancellationToken);
		return ProductDto.From(product);
	}
}
