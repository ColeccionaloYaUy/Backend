using MediatR;

namespace ColeccionaloYa.Application.Wishlist.CheckWishlist;

public record CheckWishlistQuery(int ClientId, int ProductId) : IRequest<WishlistCheckDto>;
