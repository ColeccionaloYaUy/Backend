using ColeccionaloYa.Domain.Wishlist.Exceptions;
using ColeccionaloYa.Persistence.Wishlist.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Wishlist.RemoveFromWishlist;

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Unit> {
	private readonly IWishlistRepository _Repository;

	public RemoveFromWishlistCommandHandler(IWishlistRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken) {
		if (!await _Repository.ExistsAsync(request.ClientId, request.ProductId, cancellationToken)) {
			throw new WishlistItemNotFoundException(request.ProductId);
		}

		await _Repository.RemoveAsync(request.ClientId, request.ProductId, cancellationToken);
		return Unit.Value;
	}
}
