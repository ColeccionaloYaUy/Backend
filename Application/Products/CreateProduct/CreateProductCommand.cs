using MediatR;

namespace ColeccionaloYa.Application.Products.CreateProduct;

public record CreateProductCommand(
	string Name,
	string ShortDescription,
	string LongDescription,
	decimal Price,
	string Type,
	decimal Weight,
	bool IsFeatured
) : IRequest<ProductDto>;
