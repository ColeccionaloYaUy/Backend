using ColeccionaloYa.Domain.Gallery.Exceptions;
using ColeccionaloYa.Persistence.Gallery.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Gallery.DeleteGalleryImage;

public class DeleteGalleryImageCommandHandler : IRequestHandler<DeleteGalleryImageCommand, Unit> {
	private readonly IGalleryRepository _GalleryRepository;

	public DeleteGalleryImageCommandHandler(IGalleryRepository galleryRepository) {
		_GalleryRepository = galleryRepository;
	}

	public async Task<Unit> Handle(DeleteGalleryImageCommand request, CancellationToken cancellationToken) {
		var image = await _GalleryRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new GalleryImageNotFoundException(request.Id);

		await _GalleryRepository.DeleteAsync(image.Id, cancellationToken);
		return Unit.Value;
	}
}
