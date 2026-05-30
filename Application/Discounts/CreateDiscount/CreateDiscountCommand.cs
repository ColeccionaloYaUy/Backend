using MediatR;

namespace ColeccionaloYa.Application.Discounts.CreateDiscount;

public record CreateDiscountCommand(
	int ProductId,
	decimal Porcentage,
	DateTime? ValidFrom,
	DateTime? ValidUntil
) : IRequest<DiscountDto>;
