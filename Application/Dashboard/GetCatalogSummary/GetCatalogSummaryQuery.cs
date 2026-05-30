using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetCatalogSummary;

public record GetCatalogSummaryQuery() : IRequest<CatalogSummaryDto>;
