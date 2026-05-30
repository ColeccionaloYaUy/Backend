using MediatR;

namespace ColeccionaloYa.Application.Carts.ValidateCart;

public record ValidateCartCommand(int ClientId) : IRequest<CartValidationDto>;
