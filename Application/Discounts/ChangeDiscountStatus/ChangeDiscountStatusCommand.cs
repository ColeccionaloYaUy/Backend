using MediatR;

namespace ColeccionaloYa.Application.Discounts.ChangeDiscountStatus;

public record ChangeDiscountStatusCommand(int Id, string Action) : IRequest<DiscountDto>;
