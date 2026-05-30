using ColeccionaloYa.Persistence.Packs.ReadModels;

namespace ColeccionaloYa.Application.Packs;

public record PackHeaderDto(
	int IdProduct,
	int ProductCount,
	DateTime CreationDate,
	string Name,
	string ShortDescription,
	string LongDescription,
	decimal Price,
	decimal Weight
);

public record PackProductRefDto(int IdProduct, string Name, decimal Price, string Type, string? CoverUrl) {
	public static PackProductRefDto From(PackProductRef r) =>
		new(r.IdProduct, r.Name, r.Price, r.Type, r.CoverUrl);
}

public record PackProductDto(PackProductRefDto Product, int Quantity) {
	public static PackProductDto From(PackProductItem item) =>
		new(PackProductRefDto.From(item.Product), item.Quantity);
}

public record PackDetailDto(PackHeaderDto Pack, List<PackProductDto> Products) {
	public static PackDetailDto From(PackDetailData data) =>
		new(
			new PackHeaderDto(
				data.Product.Id,
				data.Pack.ProductCount,
				data.Pack.CreationDate,
				data.Product.Name,
				data.Product.ShortDescription,
				data.Product.LongDescription,
				data.Product.Price,
				data.Product.Weight),
			data.Products.Select(PackProductDto.From).ToList());
}

public record PackItemDto(int IdProduct, int Quantity);

public record PackSummaryDto(
	int IdProduct,
	int ProductCount,
	DateTime CreationDate,
	List<PackItemDto> Items
);
