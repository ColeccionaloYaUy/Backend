using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Products.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto> {
	private readonly IProductRepository _Repository;

	public GetProductByIdQueryHandler(IProductRepository repository) {
		_Repository = repository;
	}

	public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetDetailAsync(request.Id, cancellationToken)
			?? throw new ProductNotFoundException(request.Id);
		return ProductDetailDto.From(data);
	}
}
