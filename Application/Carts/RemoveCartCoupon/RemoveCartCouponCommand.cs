using MediatR;

namespace ColeccionaloYa.Application.Carts.RemoveCartCoupon;

public record RemoveCartCouponCommand(int ClientId) : IRequest<CartDto>;
