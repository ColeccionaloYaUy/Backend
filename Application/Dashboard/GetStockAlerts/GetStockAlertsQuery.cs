using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetStockAlerts;

public record GetStockAlertsQuery(int Threshold) : IRequest<StockAlertsDto>;
