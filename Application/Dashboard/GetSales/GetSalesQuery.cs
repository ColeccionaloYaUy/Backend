using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetSales;

public record GetSalesQuery(string Period, DateOnly? DateFrom, DateOnly? DateTo, string Status) : IRequest<SalesSeriesDto>;
