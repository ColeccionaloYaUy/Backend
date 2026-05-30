using ColeccionaloYa.Domain.Products.Exceptions;

namespace ColeccionaloYa.Domain.Products;

public class Product {
	public int Id { get; internal set; }
	public string Name { get; internal set; } = string.Empty;
	public string ShortDescription { get; internal set; } = string.Empty;
	public string LongDescription { get; internal set; } = string.Empty;
	public decimal Price { get; internal set; }
	public ProductType Type { get; internal set; }
	public decimal Weight { get; internal set; }
	public bool IsFeatured { get; internal set; }
	public DateOnly CreationDate { get; internal set; }
	public bool LogicalDelete { get; internal set; }

	internal Product() { }

	public static Product Create(
		string name,
		string shortDescription,
		string longDescription,
		decimal price,
		ProductType type,
		decimal weight,
		bool isFeatured
	) {
		ValidatePrice(price);
		ValidateWeight(weight);

		return new Product {
			Name = name.Trim(),
			ShortDescription = shortDescription.Trim(),
			LongDescription = longDescription.Trim(),
			Price = price,
			Type = type,
			Weight = weight,
			IsFeatured = isFeatured,
			CreationDate = DateOnly.FromDateTime(DateTime.UtcNow),
			LogicalDelete = false,
		};
	}

	public void Update(
		string? name,
		string? shortDescription,
		string? longDescription,
		decimal? price,
		ProductType? type,
		decimal? weight,
		bool? isFeatured
	) {
		if (name is not null) Name = name.Trim();
		if (shortDescription is not null) ShortDescription = shortDescription.Trim();
		if (longDescription is not null) LongDescription = longDescription.Trim();
		if (price.HasValue) {
			ValidatePrice(price.Value);
			Price = price.Value;
		}
		if (type.HasValue) Type = type.Value;
		if (weight.HasValue) {
			ValidateWeight(weight.Value);
			Weight = weight.Value;
		}
		if (isFeatured.HasValue) IsFeatured = isFeatured.Value;
	}

	public void AssignId(int id) {
		Id = id;
	}

	public void MarkDeleted() {
		LogicalDelete = true;
	}

	private static void ValidatePrice(decimal price) {
		if (price < 0) {
			throw new InvalidProductPriceException(price);
		}
	}

	private static void ValidateWeight(decimal weight) {
		if (weight < 0) {
			throw new InvalidProductWeightException(weight);
		}
	}
}
