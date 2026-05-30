using ColeccionaloYa.Persistence.Orders.ReadModels;

namespace ColeccionaloYa.Application.Orders;

public record OrderListItemDto(
	int IdOrder,
	DateOnly CreationDate,
	int IdClient,
	string ClientName,
	string ClientEmail,
	int? IdCoupon,
	string Status,
	string Tracking,
	decimal Subtotal,
	decimal Taxes,
	decimal Total,
	int ItemsCount
) {
	public static OrderListItemDto From(OrderListItem o) =>
		new(o.IdOrder, o.CreationDate, o.IdClient, o.ClientName, o.ClientEmail, o.IdCoupon,
			o.Status, o.Tracking, o.Subtotal, o.Taxes, o.Total, o.ItemsCount);
}

public record OrderHeaderDto(
	int IdOrder,
	DateOnly CreationDate,
	int IdClient,
	int? IdCoupon,
	decimal Taxes,
	string Status,
	string Tracking,
	string Observations,
	string? CancelReason,
	string? ReturnReason,
	decimal Subtotal,
	decimal Total
);

public record OrderLineDto(int IdProduct, string ProductName, string? CoverUrl, int Quantity, decimal Price, decimal LineTotal);

public record OrderClientDto(int IdClient, string Name, string Lastname, string Email, string Phone);

public record OrderAddressDto(
	int IdSnapshot,
	string State,
	string Type,
	string? Observations,
	string? Apartment,
	string? Corner,
	string? DoorNumber,
	string? Floor,
	string Name,
	string Neighborhood,
	string Street
) {
	public static OrderAddressDto From(OrderAddressSnapshotData s) =>
		new(s.IdSnapshot, s.State, s.Type, s.Observations, s.Apartment, s.Corner, s.DoorNumber, s.Floor, s.Name, s.Neighborhood, s.Street);
}

public record OrderAddressesDto(OrderAddressDto? Delivery, OrderAddressDto? Billing);

public record OrderCouponDto(int IdCoupon, string Name, string Token, decimal Porcentage);

public record OrderDetailDto(
	OrderHeaderDto Order,
	List<OrderLineDto> Lines,
	OrderClientDto Client,
	OrderAddressesDto Addresses,
	OrderCouponDto? Coupon
) {
	public static OrderDetailDto From(OrderDetailData data) =>
		new(
			new OrderHeaderDto(
				data.Order.IdOrder, data.Order.CreationDate, data.Order.IdClient, data.Order.IdCoupon,
				data.Order.Taxes, data.Order.Status, data.Order.Tracking, data.Order.Observations,
				data.Order.CancelReason, data.Order.ReturnReason, data.Order.Subtotal, data.Order.Total),
			data.Lines.Select(l => new OrderLineDto(l.IdProduct, l.ProductName, l.CoverUrl, l.Quantity, l.Price, l.LineTotal)).ToList(),
			new OrderClientDto(data.Client.IdClient, data.Client.Name, data.Client.Lastname, data.Client.Email, data.Client.Phone),
			new OrderAddressesDto(
				data.Delivery is null ? null : OrderAddressDto.From(data.Delivery),
				data.Billing is null ? null : OrderAddressDto.From(data.Billing)),
			data.Coupon is null ? null : new OrderCouponDto(data.Coupon.IdCoupon, data.Coupon.Name, data.Coupon.Token, data.Coupon.Porcentage));
}

public record OrderStatusHistoryDto(int Id, string Status, string? Reason, int? ChangedBy, DateTime Date);

public record OrderTrackingDto(string Tracking, string Status, List<OrderStatusHistoryDto> History) {
	public static OrderTrackingDto From(OrderTrackingData data) =>
		new(data.Tracking, data.Status, data.History.Select(h => new OrderStatusHistoryDto(h.Id, h.Status, h.Reason, h.ChangedBy, h.Date)).ToList());
}
