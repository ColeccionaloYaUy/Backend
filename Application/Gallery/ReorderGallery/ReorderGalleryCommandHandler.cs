using ColeccionaloYa.Application.Products;
using ColeccionaloYa.Domain.Gallery.Exceptions;
using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Gallery.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Gallery.ReorderGallery;

public class ReorderGalleryCommandHandler : IRequestHandler<ReorderGalleryCommand, List<GalleryImageDto>> {
	private readonly IGalleryRepository _GalleryRepository;
	private readonly IProductRepository _ProductRepository;

	public ReorderGalleryCommandHandler(IGalleryRepository galleryRepository, IProductRepository productRepository) {
		_GalleryRepository = galleryRepository;
		_ProductRepository = productRepository;
	}

	public async Task<List<GalleryImageDto>> Handle(ReorderGalleryCommand request, CancellationToken cancellationToken) {
		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		var ids = request.Items.Select(i => i.IdGallery).ToList();
		if (!await _GalleryRepository.AllBelongToProductAsync(request.ProductId, ids, cancellationToken)) {
			throw new GalleryReorderMismatchException();
		}

		await _GalleryRepository.ReorderAsync(
			request.Items.Select(i => (i.IdGallery, i.Order)).ToList(),
			cancellationToken);

		var images = await _GalleryRepository.GetByProductAsync(request.ProductId, cancellationToken);
		return images.Select(GalleryImageDto.From).ToList();
	}
}
