using MediatR;

namespace ColeccionaloYa.Application.Products.ProductTags.RemoveProductTag;

public record RemoveProductTagCommand(int ProductId, int TagId) : IRequest<Unit>;
