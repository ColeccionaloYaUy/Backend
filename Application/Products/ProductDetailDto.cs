using ColeccionaloYa.Application.Tags;
using ColeccionaloYa.Domain.Gallery;
using ColeccionaloYa.Persistence.Products.ReadModels;

namespace ColeccionaloYa.Application.Products;

public record GalleryImageDto(int IdGallery, int IdProduct, int Order, string Url) {
	public static GalleryImageDto From(ProductGalleryImage g) =>
		new(g.IdGallery, g.IdProduct, g.Order, g.Url);

	public static GalleryImageDto From(GalleryImage g) =>
		new(g.Id, g.ProductId, g.Order, g.Url);
}

public record ProductStockDto(int CurrentStock, DateTime? LastMovementDate) {
	public static ProductStockDto From(ProductStockInfo s) =>
		new(s.CurrentStock, s.LastMovementDate);
}

public record ProductDetailDto(
	ProductDto Product,
	List<GalleryImageDto> Gallery,
	List<TagDto> Tags,
	DiscountInfoDto? Discount,
	ProductStockDto Stock
) {
	public static ProductDetailDto From(ProductDetailData data) =>
		new(
			ProductDto.From(data.Product),
			data.Gallery.Select(g => GalleryImageDto.From(g)).ToList(),
			data.Tags.Select(TagDto.From).ToList(),
			data.Discount is null ? null : DiscountInfoDto.From(data.Discount),
			ProductStockDto.From(data.Stock));
}
