using MediatR;

namespace ColeccionaloYa.Application.Carts.UpdateCartItem;

public record UpdateCartItemCommand(int ClientId, int ProductId, int Quantity) : IRequest<CartDto>;
