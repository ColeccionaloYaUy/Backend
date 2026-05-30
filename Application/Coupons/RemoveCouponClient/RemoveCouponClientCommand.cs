using MediatR;

namespace ColeccionaloYa.Application.Coupons.RemoveCouponClient;

public record RemoveCouponClientCommand(int CouponId, int ClientId) : IRequest<Unit>;
