using MediatR;

namespace ColeccionaloYa.Application.Coupons.DeleteCoupon;

public record DeleteCouponCommand(int Id) : IRequest<Unit>;
