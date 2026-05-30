using MediatR;

namespace ColeccionaloYa.Application.Products.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductDetailDto>;
