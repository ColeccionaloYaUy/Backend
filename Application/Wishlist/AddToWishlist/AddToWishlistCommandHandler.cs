using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Domain.Wishlist.Exceptions;
using ColeccionaloYa.Persistence.Products.Interfaces;
using ColeccionaloYa.Persistence.Wishlist.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Wishlist.AddToWishlist;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, WishlistAddedDto> {
	private readonly IWishlistRepository _Repository;
	private readonly IProductRepository _ProductRepository;

	public AddToWishlistCommandHandler(IWishlistRepository repository, IProductRepository productRepository) {
		_Repository = repository;
		_ProductRepository = productRepository;
	}

	public async Task<WishlistAddedDto> Handle(AddToWishlistCommand request, CancellationToken cancellationToken) {
		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		if (await _Repository.ExistsAsync(request.ClientId, request.ProductId, cancellationToken)) {
			throw new WishlistItemAlreadyExistsException(request.ProductId);
		}

		await _Repository.AddAsync(request.ClientId, request.ProductId, cancellationToken);
		return new WishlistAddedDto(request.ClientId, request.ProductId);
	}
}
