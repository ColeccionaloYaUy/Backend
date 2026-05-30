using MediatR;

namespace ColeccionaloYa.Application.Coupons.UpdateCoupon;

public record UpdateCouponCommand(
	int Id,
	string? Name,
	string? Description,
	DateTime? ValidFrom,
	DateTime? ValidUntil,
	decimal? Porcentage
) : IRequest<CouponDto>;
