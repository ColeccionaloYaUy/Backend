using MediatR;

namespace ColeccionaloYa.Application.Wishlist.GetWishlist;

public record GetWishlistQuery(int ClientId) : IRequest<List<WishlistItemDto>>;
