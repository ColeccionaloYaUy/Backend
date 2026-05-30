using MediatR;

namespace ColeccionaloYa.Application.Discounts.UpdateDiscount;

public record UpdateDiscountCommand(
	int Id,
	decimal? Porcentage,
	DateTime? ValidFrom,
	DateTime? ValidUntil
) : IRequest<DiscountDto>;
