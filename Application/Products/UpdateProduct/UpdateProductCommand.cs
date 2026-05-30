using MediatR;

namespace ColeccionaloYa.Application.Products.UpdateProduct;

public record UpdateProductCommand(
	int Id,
	string? Name,
	string? ShortDescription,
	string? LongDescription,
	decimal? Price,
	string? Type,
	decimal? Weight,
	bool? IsFeatured
) : IRequest<ProductDto>;
