using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Products.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto> {
	private readonly IProductRepository _Repository;

	public CreateProductCommandHandler(IProductRepository repository) {
		_Repository = repository;
	}

	public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken) {
		var type = ProductTypeParser.Parse(request.Type);

		var product = Product.Create(
			request.Name,
			request.ShortDescription,
			request.LongDescription,
			request.Price,
			type,
			request.Weight,
			request.IsFeatured);

		await _Repository.CreateAsync(product, cancellationToken);
		return ProductDto.From(product);
	}
}
