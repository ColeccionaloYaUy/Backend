using ColeccionaloYa.Domain.Stock;

namespace ColeccionaloYa.Persistence.Stock.ReadModels;

public record StockMovementWithProduct(StockMovement Movement, string ProductName);

public record StockSummary(
	int ProductId,
	int CurrentStock,
	int TotalInput,
	int TotalOutput,
	DateTime? LastMovementDate
);

public record StockMovementHistoryItem(StockMovement Movement, int RunningBalance);
