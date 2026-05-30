using MediatR;

namespace ColeccionaloYa.Application.Carts.ApplyCoupon;

public record ApplyCouponCommand(int ClientId, string CouponToken) : IRequest<ApplyCouponResultDto>;
