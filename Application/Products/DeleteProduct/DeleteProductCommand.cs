using MediatR;

namespace ColeccionaloYa.Application.Products.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<Unit>;
