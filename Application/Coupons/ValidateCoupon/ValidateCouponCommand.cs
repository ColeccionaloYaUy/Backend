using MediatR;

namespace ColeccionaloYa.Application.Coupons.ValidateCoupon;

public record ValidateCouponCommand(int ClientId, string Token) : IRequest<CouponValidateDto>;
