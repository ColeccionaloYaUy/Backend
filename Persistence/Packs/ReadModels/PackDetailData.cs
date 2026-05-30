using ColeccionaloYa.Domain.Packs;
using ColeccionaloYa.Domain.Products;

namespace ColeccionaloYa.Persistence.Packs.ReadModels;

public record PackProductRef(
	int IdProduct,
	string Name,
	decimal Price,
	string Type,
	string? CoverUrl
);

public record PackProductItem(PackProductRef Product, int Quantity);

public record PackDetailData(
	Pack Pack,
	Product Product,
	List<PackProductItem> Products
);
