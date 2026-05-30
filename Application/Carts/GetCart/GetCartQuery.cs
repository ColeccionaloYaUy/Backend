using MediatR;

namespace ColeccionaloYa.Application.Carts.GetCart;

public record GetCartQuery(int ClientId) : IRequest<CartDto>;
