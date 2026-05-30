using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Products.ProductTags.RemoveProductTag;

public class RemoveProductTagCommandHandler : IRequestHandler<RemoveProductTagCommand, Unit> {
	private readonly IProductTagRepository _ProductTagRepository;

	public RemoveProductTagCommandHandler(IProductTagRepository productTagRepository) {
		_ProductTagRepository = productTagRepository;
	}

	public async Task<Unit> Handle(RemoveProductTagCommand request, CancellationToken cancellationToken) {
		if (!await _ProductTagRepository.LinkExistsAsync(request.ProductId, request.TagId, cancellationToken)) {
			throw new ProductTagNotLinkedException(request.ProductId, request.TagId);
		}

		await _ProductTagRepository.RemoveAsync(request.ProductId, request.TagId, cancellationToken);
		return Unit.Value;
	}
}
