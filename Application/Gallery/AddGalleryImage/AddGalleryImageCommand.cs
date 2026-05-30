using ColeccionaloYa.Application.Products;
using MediatR;

namespace ColeccionaloYa.Application.Gallery.AddGalleryImage;

public record AddGalleryImageCommand(
	int ProductId,
	byte[]? FileBytes,
	string? FileName,
	string? Url,
	int? Order
) : IRequest<GalleryImageDto>;
