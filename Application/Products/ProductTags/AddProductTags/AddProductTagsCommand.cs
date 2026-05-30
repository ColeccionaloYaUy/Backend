using MediatR;

namespace ColeccionaloYa.Application.Products.ProductTags.AddProductTags;

public record AddProductTagsCommand(int ProductId, IReadOnlyList<int> TagIds) : IRequest<ProductTagsDto>;
