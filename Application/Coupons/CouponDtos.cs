using ColeccionaloYa.Domain.Coupons;
using ColeccionaloYa.Persistence.Coupons.ReadModels;

namespace ColeccionaloYa.Application.Coupons;

public record CouponDto(
	int IdCoupon,
	string Name,
	string Token,
	string Description,
	DateTime CreationDate,
	DateTime? ValidFrom,
	DateTime? ValidUntil,
	decimal Porcentage,
	bool IsActive
) {
	public static CouponDto From(Coupon c) =>
		new(c.Id, c.Name, c.Token, c.Description, c.CreationDate, c.ValidFrom, c.ValidUntil, c.Porcentage, c.IsActive);
}

public record CouponListItemDto(
	int IdCoupon,
	string Name,
	string Token,
	string Description,
	DateTime CreationDate,
	DateTime? ValidFrom,
	DateTime? ValidUntil,
	decimal Porcentage,
	bool IsActive,
	int AssignedClientsCount
) {
	public static CouponListItemDto From(CouponWithCount c) =>
		new(c.Coupon.Id, c.Coupon.Name, c.Coupon.Token, c.Coupon.Description, c.Coupon.CreationDate,
			c.Coupon.ValidFrom, c.Coupon.ValidUntil, c.Coupon.Porcentage, c.Coupon.IsActive, c.AssignedClientsCount);
}

public record CouponClientDto(int IdClient, string Name, string Lastname, string Email) {
	public static CouponClientDto From(CouponClientBrief c) =>
		new(c.IdClient, c.Name, c.Lastname, c.Email);
}

public record CouponDetailDto(CouponDto Coupon, List<CouponClientDto> Clients) {
	public static CouponDetailDto From(CouponWithClients data) =>
		new(CouponDto.From(data.Coupon), data.Clients.Select(CouponClientDto.From).ToList());
}

public record CouponMeDto(
	int IdCoupon,
	string Name,
	string Token,
	string Description,
	DateTime? ValidFrom,
	DateTime? ValidUntil,
	decimal Porcentage
) {
	public static CouponMeDto From(Coupon c) =>
		new(c.Id, c.Name, c.Token, c.Description, c.ValidFrom, c.ValidUntil, c.Porcentage);
}

public record CouponValidateCouponDto(
	int IdCoupon,
	string Name,
	string Token,
	decimal Porcentage,
	DateTime? ValidFrom,
	DateTime? ValidUntil
) {
	public static CouponValidateCouponDto From(Coupon c) =>
		new(c.Id, c.Name, c.Token, c.Porcentage, c.ValidFrom, c.ValidUntil);
}

public record CouponValidateDto(bool Valid, string? Reason, CouponValidateCouponDto? Coupon, decimal Discount);

public record CouponClientsDto(int IdCoupon, List<CouponClientDto> Clients);
