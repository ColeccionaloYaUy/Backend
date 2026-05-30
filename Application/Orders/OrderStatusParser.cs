using ColeccionaloYa.Domain.Orders;
using ColeccionaloYa.Domain.Orders.Exceptions;

namespace ColeccionaloYa.Application.Orders;

public static class OrderStatusParser {
	public static OrderStatus Parse(string status) {
		if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed)) {
			throw new InvalidOrderStatusException(status);
		}
		return parsed;
	}

	public static string? ToDbString(string? status) =>
		status is null ? null : Parse(status).ToString().ToLowerInvariant();
}
