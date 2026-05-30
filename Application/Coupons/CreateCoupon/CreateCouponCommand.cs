using MediatR;

namespace ColeccionaloYa.Application.Coupons.CreateCoupon;

public record CreateCouponCommand(
	string Name,
	string Token,
	string Description,
	DateTime? ValidFrom,
	DateTime? ValidUntil,
	decimal Porcentage
) : IRequest<CouponDto>;
