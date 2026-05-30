using ColeccionaloYa.Domain.Stock.Exceptions;

namespace ColeccionaloYa.Domain.Stock;

public class StockMovement {
	public int Id { get; internal set; }
	public int ProductId { get; internal set; }
	public DateTime Date { get; internal set; }
	public int Input { get; internal set; }
	public int Output { get; internal set; }
	public int? OrderId { get; internal set; }
	public StockMovementType Type { get; internal set; }
	public string? Reason { get; internal set; }

	internal StockMovement() { }

	public static StockMovement Create(int productId, StockMovementType type, int quantity, DateTime date, int? orderId, string? reason) {
		if (quantity == 0) {
			throw new InvalidStockQuantityException();
		}

		int input;
		int output;
		switch (type) {
			case StockMovementType.Entry:
				if (quantity < 0) {
					throw new InvalidStockSignException(type);
				}
				input = quantity;
				output = 0;
				break;
			case StockMovementType.Exit:
				if (quantity < 0) {
					throw new InvalidStockSignException(type);
				}
				input = 0;
				output = quantity;
				break;
			case StockMovementType.Adjustment:
				if (quantity > 0) {
					input = quantity;
					output = 0;
				} else {
					input = 0;
					output = -quantity;
				}
				break;
			default:
				throw new InvalidStockSignException(type);
		}

		return new StockMovement {
			ProductId = productId,
			Type = type,
			Input = input,
			Output = output,
			Date = date,
			OrderId = orderId,
			Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
		};
	}

	public void EnsureDoesNotCauseNegativeStock(int currentStock) {
		if (currentStock - Output < 0) {
			throw new NegativeStockException(ProductId);
		}
	}

	public void AssignId(int id) {
		Id = id;
	}
}
