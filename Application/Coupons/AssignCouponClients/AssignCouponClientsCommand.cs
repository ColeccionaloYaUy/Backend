using MediatR;

namespace ColeccionaloYa.Application.Coupons.AssignCouponClients;

public record AssignCouponClientsCommand(int CouponId, IReadOnlyList<int> ClientIds) : IRequest<CouponClientsDto>;
