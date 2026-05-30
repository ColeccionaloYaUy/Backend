using MediatR;

namespace ColeccionaloYa.Application.Carts.AddCartItem;

public record AddCartItemCommand(int ClientId, int ProductId, int Quantity) : IRequest<CartDto>;
