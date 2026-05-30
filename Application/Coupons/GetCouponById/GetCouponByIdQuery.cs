using MediatR;

namespace ColeccionaloYa.Application.Coupons.GetCouponById;

public record GetCouponByIdQuery(int Id) : IRequest<CouponDetailDto>;
