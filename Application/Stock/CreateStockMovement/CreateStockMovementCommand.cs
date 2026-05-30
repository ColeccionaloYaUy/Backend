using MediatR;

namespace ColeccionaloYa.Application.Stock.CreateStockMovement;

public record CreateStockMovementCommand(
	int ProductId,
	string Type,
	int Quantity,
	DateTime Date,
	int? OrderId,
	string? Reason
) : IRequest<StockMovementCreatedDto>;
