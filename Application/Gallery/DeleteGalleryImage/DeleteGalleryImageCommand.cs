using MediatR;

namespace ColeccionaloYa.Application.Gallery.DeleteGalleryImage;

public record DeleteGalleryImageCommand(int Id) : IRequest<Unit>;
