using MediatR;

namespace ColeccionaloYa.Application.Coupons.DeactivateCoupon;

public record DeactivateCouponCommand(int Id) : IRequest<CouponDto>;
