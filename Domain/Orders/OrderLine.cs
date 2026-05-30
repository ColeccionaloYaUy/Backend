namespace ColeccionaloYa.Domain.Orders;

public class OrderLine {
	public int ProductId { get; internal set; }
	public int Quantity { get; internal set; }
	public decimal Price { get; internal set; }

	internal OrderLine() { }

	public static OrderLine Create(int productId, int quantity, decimal price) {
		return new OrderLine {
			ProductId = productId,
			Quantity = quantity,
			Price = price,
		};
	}
}
