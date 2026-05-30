using MediatR;

namespace ColeccionaloYa.Application.Stock.GetProductStock;

public record GetProductStockQuery(int ProductId) : IRequest<StockSummaryDto>;
