using ColeccionaloYa.Domain.Discounts.Exceptions;

namespace ColeccionaloYa.Domain.Discounts;

public class Discount {
	public int Id { get; internal set; }
	public int ProductId { get; internal set; }
	public decimal Porcentage { get; internal set; }
	public DateTime CreationDate { get; internal set; }
	public DateTime? ValidFrom { get; internal set; }
	public DateTime? ValidUntil { get; internal set; }
	public bool IsActive { get; internal set; }
	public bool LogicalDelete { get; internal set; }

	internal Discount() { }

	public static Discount Create(int productId, decimal porcentage, DateTime? validFrom, DateTime? validUntil) {
		ValidatePorcentage(porcentage);

		return new Discount {
			ProductId = productId,
			Porcentage = porcentage,
			CreationDate = DateTime.UtcNow,
			ValidFrom = validFrom,
			ValidUntil = validUntil,
			IsActive = false,
			LogicalDelete = false,
		};
	}

	public void Update(decimal? porcentage, DateTime? validFrom, DateTime? validUntil) {
		if (porcentage.HasValue) {
			ValidatePorcentage(porcentage.Value);
			Porcentage = porcentage.Value;
		}
		ValidFrom = validFrom;
		ValidUntil = validUntil;
	}

	public void Activate() {
		IsActive = true;
	}

	public void Deactivate() {
		IsActive = false;
	}

	public void AssignId(int id) {
		Id = id;
	}

	private static void ValidatePorcentage(decimal porcentage) {
		if (porcentage < 0 || porcentage > 100) {
			throw new InvalidDiscountPercentageException(porcentage);
		}
	}
}
