using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Domain.Tags.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Persistence.Tags.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Products.ProductTags.AddProductTags;

public class AddProductTagsCommandHandler : IRequestHandler<AddProductTagsCommand, ProductTagsDto> {
	private readonly IProductRepository _ProductRepository;
	private readonly IProductTagRepository _ProductTagRepository;
	private readonly ITagRepository _TagRepository;

	public AddProductTagsCommandHandler(
		IProductRepository productRepository,
		IProductTagRepository productTagRepository,
		ITagRepository tagRepository) {
		_ProductRepository = productRepository;
		_ProductTagRepository = productTagRepository;
		_TagRepository = tagRepository;
	}

	public async Task<ProductTagsDto> Handle(AddProductTagsCommand request, CancellationToken cancellationToken) {
		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		if (!await _TagRepository.ExistByIdsAsync(request.TagIds, cancellationToken)) {
			throw new InvalidTagReferenceException();
		}

		if (await _ProductTagRepository.AnyLinkedAsync(request.ProductId, request.TagIds, cancellationToken)) {
			throw new ProductTagAlreadyLinkedException();
		}

		await _ProductTagRepository.AddAsync(request.ProductId, request.TagIds, cancellationToken);
		var tags = await _ProductTagRepository.GetTagsAsync(request.ProductId, cancellationToken);
		return ProductTagsDto.From(request.ProductId, tags);
	}
}
