using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetSummary;

public record GetSummaryQuery(DateOnly? DateFrom, DateOnly? DateTo) : IRequest<DashboardSummaryDto>;
