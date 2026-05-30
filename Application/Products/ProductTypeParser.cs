using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Domain.Products.Exceptions;

namespace ColeccionaloYa.Application.Products;

public static class ProductTypeParser {
	public static ProductType Parse(string type) {
		if (!Enum.TryParse<ProductType>(type, ignoreCase: true, out var parsed)
			|| !Enum.IsDefined(parsed)) {
			throw new InvalidProductTypeException(type);
		}
		return parsed;
	}

	public static ProductType? ParseOptional(string? type) {
		if (type is null) {
			return null;
		}
		return Parse(type);
	}
}
