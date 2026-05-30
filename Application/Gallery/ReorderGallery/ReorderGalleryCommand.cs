using ColeccionaloYa.Application.Products;
using MediatR;

namespace ColeccionaloYa.Application.Gallery.ReorderGallery;

public record GalleryReorderItem(int IdGallery, int Order);

public record ReorderGalleryCommand(int ProductId, IReadOnlyList<GalleryReorderItem> Items)
	: IRequest<List<GalleryImageDto>>;
