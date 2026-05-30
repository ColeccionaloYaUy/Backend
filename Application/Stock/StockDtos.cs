using ColeccionaloYa.Domain.Stock;
using ColeccionaloYa.Persistence.Stock.ReadModels;

namespace ColeccionaloYa.Application.Stock;

public record StockMovementDto(
	int IdStock,
	int IdProduct,
	string ProductName,
	DateTime Date,
	int Input,
	int Output,
	int? IdOrder,
	string Type,
	string? Reason
) {
	public static StockMovementDto From(StockMovementWithProduct m) =>
		new(m.Movement.Id, m.Movement.ProductId, m.ProductName, m.Movement.Date, m.Movement.Input,
			m.Movement.Output, m.Movement.OrderId, m.Movement.Type.ToString().ToLowerInvariant(), m.Movement.Reason);
}

public record StockSummaryDto(
	int IdProduct,
	int CurrentStock,
	int TotalInput,
	int TotalOutput,
	DateTime? LastMovementDate
) {
	public static StockSummaryDto From(StockSummary s) =>
		new(s.ProductId, s.CurrentStock, s.TotalInput, s.TotalOutput, s.LastMovementDate);
}

public record StockMovementHistoryDto(
	int IdStock,
	DateTime Date,
	int Input,
	int Output,
	int? IdOrder,
	string Type,
	string? Reason,
	int RunningBalance
) {
	public static StockMovementHistoryDto From(StockMovementHistoryItem m) =>
		new(m.Movement.Id, m.Movement.Date, m.Movement.Input, m.Movement.Output, m.Movement.OrderId,
			m.Movement.Type.ToString().ToLowerInvariant(), m.Movement.Reason, m.RunningBalance);
}

public record StockMovementCreatedDto(
	int IdStock,
	int IdProduct,
	DateTime Date,
	int Input,
	int Output,
	int? IdOrder,
	string Type,
	string? Reason
) {
	public static StockMovementCreatedDto From(StockMovement m) =>
		new(m.Id, m.ProductId, m.Date, m.Input, m.Output, m.OrderId, m.Type.ToString().ToLowerInvariant(), m.Reason);
}
