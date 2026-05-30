using MediatR;

namespace ColeccionaloYa.Application.Carts.ClearCart;

public record ClearCartCommand(int ClientId) : IRequest<Unit>;
