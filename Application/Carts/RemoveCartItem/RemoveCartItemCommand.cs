using MediatR;

namespace ColeccionaloYa.Application.Carts.RemoveCartItem;

public record RemoveCartItemCommand(int ClientId, int ProductId) : IRequest<CartDto>;
