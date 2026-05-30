using ColeccionaloYa.Application.Files;
using ColeccionaloYa.Application.Products;
using ColeccionaloYa.Domain.Gallery;
using ColeccionaloYa.Domain.Gallery.Exceptions;
using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Gallery.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Persistence.Storage.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Gallery.AddGalleryImage;

public class AddGalleryImageCommandHandler : IRequestHandler<AddGalleryImageCommand, GalleryImageDto> {
	private readonly IGalleryRepository _GalleryRepository;
	private readonly IProductRepository _ProductRepository;
	private readonly IFileStorage _FileStorage;

	public AddGalleryImageCommandHandler(
		IGalleryRepository galleryRepository,
		IProductRepository productRepository,
		IFileStorage fileStorage) {
		_GalleryRepository = galleryRepository;
		_ProductRepository = productRepository;
		_FileStorage = fileStorage;
	}

	public async Task<GalleryImageDto> Handle(AddGalleryImageCommand request, CancellationToken cancellationToken) {
		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		string url;
		if (request.FileBytes is not null && request.FileBytes.Length > 0) {
			var fileName = request.FileName ?? "image";
			ImageValidation.Validate(request.FileBytes, fileName);
			var stored = await _FileStorage.SaveAsync(request.FileBytes, fileName, cancellationToken);
			url = stored.Url;
		} else if (!string.IsNullOrWhiteSpace(request.Url)) {
			url = request.Url.Trim();
		} else {
			throw new GalleryImageSourceRequiredException();
		}

		var order = request.Order ?? await _GalleryRepository.GetNextOrderAsync(request.ProductId, cancellationToken);

		var image = GalleryImage.Create(request.ProductId, order, url);
		await _GalleryRepository.CreateAsync(image, cancellationToken);
		return GalleryImageDto.From(image);
	}
}
