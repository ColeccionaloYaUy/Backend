using ColeccionaloYa.Domain.Orders.Exceptions;

namespace ColeccionaloYa.Domain.Orders;

public class Order {
	private readonly List<OrderLine> _lines = new();

	public int Id { get; internal set; }
	public DateOnly CreationDate { get; internal set; }
	public int ClientId { get; internal set; }
	public int? CouponId { get; internal set; }
	public decimal Taxes { get; internal set; }
	public OrderStatus Status { get; internal set; }
	public string Tracking { get; internal set; } = string.Empty;
	public string Observations { get; internal set; } = string.Empty;
	public string? CancelReason { get; internal set; }
	public string? ReturnReason { get; internal set; }
	public decimal Subtotal { get; internal set; }
	public decimal Total { get; internal set; }
	public int? DeliverySnapshotId { get; internal set; }
	public int? OrderSnapshotId { get; internal set; }

	public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

	internal Order() { }

	public static Order Create(
		int clientId,
		int? couponId,
		IEnumerable<OrderLine> lines,
		decimal subtotal,
		decimal taxes,
		decimal total,
		string? observations
	) {
		var list = lines.ToList();
		if (list.Count == 0) {
			throw new EmptyOrderException();
		}

		var order = new Order {
			ClientId = clientId,
			CouponId = couponId,
			CreationDate = DateOnly.FromDateTime(DateTime.UtcNow),
			Status = OrderStatus.Pending,
			Tracking = string.Empty,
			Observations = observations?.Trim() ?? string.Empty,
			Subtotal = subtotal,
			Taxes = taxes,
			Total = total,
		};
		order._lines.AddRange(list);
		return order;
	}

	public void ChangeStatus(OrderStatus target) {
		if (!IsTransitionAllowed(Status, target)) {
			throw new OrderInvalidTransitionException(Id, Status, target);
		}
		Status = target;
	}

	public void Cancel(string reason) {
		if (Status is OrderStatus.Cancelled or OrderStatus.Delivered or OrderStatus.Returned) {
			throw new OrderCannotBeCancelledException(Id);
		}
		Status = OrderStatus.Cancelled;
		CancelReason = reason.Trim();
	}

	public void MarkReturned(string reason) {
		if (Status != OrderStatus.Delivered) {
			throw new OrderCannotBeReturnedException(Id);
		}
		Status = OrderStatus.Returned;
		ReturnReason = reason.Trim();
	}

	public void SetTracking(string tracking) {
		Tracking = tracking.Trim();
	}

	public void AssignId(int id) {
		Id = id;
	}

	public void AssignSnapshots(int deliverySnapshotId, int orderSnapshotId) {
		DeliverySnapshotId = deliverySnapshotId;
		OrderSnapshotId = orderSnapshotId;
	}

	private static bool IsTransitionAllowed(OrderStatus from, OrderStatus to) {
		return from switch {
			OrderStatus.Pending => to is OrderStatus.Confirmed or OrderStatus.Cancelled,
			OrderStatus.Confirmed => to is OrderStatus.Processing or OrderStatus.Cancelled,
			OrderStatus.Processing => to is OrderStatus.Shipped or OrderStatus.Cancelled,
			OrderStatus.Shipped => to is OrderStatus.Delivered,
			OrderStatus.Delivered => to is OrderStatus.Returned,
			_ => false,
		};
	}
}
