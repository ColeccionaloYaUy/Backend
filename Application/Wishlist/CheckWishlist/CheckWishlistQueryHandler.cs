using ColeccionaloYa.Persistence.Wishlist.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Wishlist.CheckWishlist;

public class CheckWishlistQueryHandler : IRequestHandler<CheckWishlistQuery, WishlistCheckDto> {
	private readonly IWishlistRepository _Repository;

	public CheckWishlistQueryHandler(IWishlistRepository repository) {
		_Repository = repository;
	}

	public async Task<WishlistCheckDto> Handle(CheckWishlistQuery request, CancellationToken cancellationToken) {
		var exists = await _Repository.ExistsAsync(request.ClientId, request.ProductId, cancellationToken);
		return new WishlistCheckDto(exists);
	}
}
