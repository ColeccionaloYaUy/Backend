using ColeccionaloYa.Domain.Coupons;

namespace ColeccionaloYa.Persistence.Coupons.ReadModels;

public record CouponWithCount(Coupon Coupon, int AssignedClientsCount);

public record CouponClientBrief(int IdClient, string Name, string Lastname, string Email);

public record CouponWithClients(Coupon Coupon, List<CouponClientBrief> Clients);
