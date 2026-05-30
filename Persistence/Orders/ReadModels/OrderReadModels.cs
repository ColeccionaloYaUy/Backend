namespace ColeccionaloYa.Persistence.Orders.ReadModels;

public record OrderListItem(
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
);

public record OrderHeaderData(
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

public record OrderLineDetail(
	int IdProduct,
	string ProductName,
	string? CoverUrl,
	int Quantity,
	decimal Price,
	decimal LineTotal
);

public record OrderClientInfo(int IdClient, string Name, string Lastname, string Email, string Phone);

public record OrderAddressSnapshotData(
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
);

public record OrderCouponInfo(int IdCoupon, string Name, string Token, decimal Porcentage);

public record OrderDetailData(
	OrderHeaderData Order,
	List<OrderLineDetail> Lines,
	OrderClientInfo Client,
	OrderAddressSnapshotData? Delivery,
	OrderAddressSnapshotData? Billing,
	OrderCouponInfo? Coupon
);

public record OrderStatusHistoryEntry(int Id, string Status, string? Reason, int? ChangedBy, DateTime Date);

public record OrderTrackingData(string Tracking, string Status, List<OrderStatusHistoryEntry> History);
