using ColeccionaloYa.Domain.Coupons.Exceptions;

namespace ColeccionaloYa.Domain.Coupons;

public class Coupon {
	public int Id { get; internal set; }
	public string Name { get; internal set; } = string.Empty;
	public string Token { get; internal set; } = string.Empty;
	public string Description { get; internal set; } = string.Empty;
	public DateTime CreationDate { get; internal set; }
	public DateTime? ValidFrom { get; internal set; }
	public DateTime? ValidUntil { get; internal set; }
	public decimal Porcentage { get; internal set; }
	public bool IsActive { get; internal set; }
	public bool LogicalDelete { get; internal set; }

	internal Coupon() { }

	public static Coupon Create(string name, string token, string description, DateTime? validFrom, DateTime? validUntil, decimal porcentage) {
		ValidatePorcentage(porcentage);

		return new Coupon {
			Name = name.Trim(),
			Token = token.Trim(),
			Description = description.Trim(),
			CreationDate = DateTime.UtcNow,
			ValidFrom = validFrom,
			ValidUntil = validUntil,
			Porcentage = porcentage,
			IsActive = false,
			LogicalDelete = false,
		};
	}

	public void Update(string? name, string? description, DateTime? validFrom, DateTime? validUntil, decimal? porcentage) {
		if (name is not null) Name = name.Trim();
		if (description is not null) Description = description.Trim();
		ValidFrom = validFrom;
		ValidUntil = validUntil;
		if (porcentage.HasValue) {
			ValidatePorcentage(porcentage.Value);
			Porcentage = porcentage.Value;
		}
	}

	public void Activate() {
		if (IsActive) {
			throw new CouponAlreadyActiveException(Id);
		}
		if (IsExpired(DateTime.UtcNow)) {
			throw new CouponExpiredException(Id);
		}
		IsActive = true;
	}

	public void Deactivate() {
		if (!IsActive) {
			throw new CouponAlreadyInactiveException(Id);
		}
		IsActive = false;
	}

	public bool IsExpired(DateTime now) => ValidUntil.HasValue && ValidUntil.Value < now;

	public bool IsWithinWindow(DateTime now) =>
		(!ValidFrom.HasValue || ValidFrom.Value <= now) && (!ValidUntil.HasValue || ValidUntil.Value >= now);

	public void AssignId(int id) {
		Id = id;
	}

	private static void ValidatePorcentage(decimal porcentage) {
		if (porcentage < 0 || porcentage > 100) {
			throw new InvalidCouponPercentageException(porcentage);
		}
	}
}
