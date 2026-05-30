using MediatR;

namespace ColeccionaloYa.Application.Dashboard.GetTopProducts;

public record GetTopProductsQuery(int Limit, DateOnly? DateFrom, DateOnly? DateTo, string? Type) : IRequest<TopProductsDto>;
