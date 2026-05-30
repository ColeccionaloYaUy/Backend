using MediatR;

namespace ColeccionaloYa.Application.Wishlist.RemoveFromWishlist;

public record RemoveFromWishlistCommand(int ClientId, int ProductId) : IRequest<Unit>;
