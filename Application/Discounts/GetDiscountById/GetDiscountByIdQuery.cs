using MediatR;

namespace ColeccionaloYa.Application.Discounts.GetDiscountById;

public record GetDiscountByIdQuery(int Id) : IRequest<DiscountDto>;
