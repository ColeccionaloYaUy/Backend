using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Domain.Tags;

namespace ColeccionaloYa.Persistence.Products.ReadModels;

public record ProductDetailData(
	Product Product,
	List<ProductGalleryImage> Gallery,
	List<Tag> Tags,
	ActiveDiscountInfo? Discount,
	ProductStockInfo Stock
);
