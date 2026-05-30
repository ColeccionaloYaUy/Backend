using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetRealtimeSummary;

public record GetRealtimeSummaryQuery() : IRequest<RealtimeSummaryDto>;
