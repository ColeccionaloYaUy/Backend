using MediatR;

namespace ColeccionaloYa.Application.Coupons.GetMyCoupons;

public record GetMyCouponsQuery(int ClientId) : IRequest<List<CouponMeDto>>;
