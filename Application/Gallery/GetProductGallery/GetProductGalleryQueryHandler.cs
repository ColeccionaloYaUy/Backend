using ColeccionaloYa.Application.Products;
using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Gallery.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Gallery.GetProductGallery;

public class GetProductGalleryQueryHandler : IRequestHandler<GetProductGalleryQuery, List<GalleryImageDto>> {
	private readonly IGalleryRepository _GalleryRepository;
	private readonly IProductRepository _ProductRepository;

	public GetProductGalleryQueryHandler(IGalleryRepository galleryRepository, IProductRepository productRepository) {
		_GalleryRepository = galleryRepository;
		_ProductRepository = productRepository;
	}

	public async Task<List<GalleryImageDto>> Handle(GetProductGalleryQuery request, CancellationToken cancellationToken) {
		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		var images = await _GalleryRepository.GetByProductAsync(request.ProductId, cancellationToken);
		return images.Select(GalleryImageDto.From).ToList();
	}
}
