using ColeccionaloYa.Application.Products;
using MediatR;

namespace ColeccionaloYa.Application.Gallery.GetProductGallery;

public record GetProductGalleryQuery(int ProductId) : IRequest<List<GalleryImageDto>>;
