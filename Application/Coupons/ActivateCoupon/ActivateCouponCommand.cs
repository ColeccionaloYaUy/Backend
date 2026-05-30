using MediatR;

namespace ColeccionaloYa.Application.Coupons.ActivateCoupon;

public record ActivateCouponCommand(int Id) : IRequest<CouponDto>;
