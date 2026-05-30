using ColeccionaloYa.Domain.Discounts;
using ColeccionaloYa.Domain.Discounts.Exceptions;
using ColeccionaloYa.Domain.Products.Exceptions;
using ColeccionaloYa.Persistence.Discounts.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Discounts.CreateDiscount;

public class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, DiscountDto> {
	private readonly IDiscountRepository _Repository;
	private readonly IProductRepository _ProductRepository;

	public CreateDiscountCommandHandler(IDiscountRepository repository, IProductRepository productRepository) {
		_Repository = repository;
		_ProductRepository = productRepository;
	}

	public async Task<DiscountDto> Handle(CreateDiscountCommand request, CancellationToken cancellationToken) {
		if (!await _ProductRepository.ExistsAsync(request.ProductId, cancellationToken)) {
			throw new ProductNotFoundException(request.ProductId);
		}

		if (await _Repository.HasOverlappingActiveAsync(request.ProductId, request.ValidFrom, request.ValidUntil, null, cancellationToken)) {
			throw new DiscountOverlapException(request.ProductId);
		}

		var discount = Discount.Create(request.ProductId, request.Porcentage, request.ValidFrom, request.ValidUntil);
		await _Repository.CreateAsync(discount, cancellationToken);

		var data = await _Repository.GetByIdAsync(discount.Id, cancellationToken)
			?? throw new DiscountNotFoundException(discount.Id);
		return DiscountDto.From(data);
	}
}
