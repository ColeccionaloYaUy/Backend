using MediatR;

namespace ColeccionaloYa.Application.Wishlist.AddToWishlist;

public record AddToWishlistCommand(int ClientId, int ProductId) : IRequest<WishlistAddedDto>;
