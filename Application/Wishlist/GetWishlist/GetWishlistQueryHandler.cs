using ColeccionaloYa.Persistence.Wishlist.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Wishlist.GetWishlist;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, List<WishlistItemDto>> {
	private readonly IWishlistRepository _Repository;

	public GetWishlistQueryHandler(IWishlistRepository repository) {
		_Repository = repository;
	}

	public async Task<List<WishlistItemDto>> Handle(GetWishlistQuery request, CancellationToken cancellationToken) {
		var items = await _Repository.GetByClientAsync(request.ClientId, cancellationToken);
		return items.Select(WishlistItemDto.From).ToList();
	}
}
