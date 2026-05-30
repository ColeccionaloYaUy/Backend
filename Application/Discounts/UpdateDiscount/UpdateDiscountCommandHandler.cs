using ColeccionaloYa.Domain.Discounts.Exceptions;
using ColeccionaloYa.Persistence.Discounts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Discounts.UpdateDiscount;

public class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCommand, DiscountDto> {
	private readonly IDiscountRepository _Repository;

	public UpdateDiscountCommandHandler(IDiscountRepository repository) {
		_Repository = repository;
	}

	public async Task<DiscountDto> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken) {
		var discount = await _Repository.GetEntityAsync(request.Id, cancellationToken)
			?? throw new DiscountNotFoundException(request.Id);

		discount.Update(request.Porcentage, request.ValidFrom, request.ValidUntil);

		if (discount.IsActive
			&& await _Repository.HasOverlappingActiveAsync(discount.ProductId, discount.ValidFrom, discount.ValidUntil, discount.Id, cancellationToken)) {
			throw new DiscountOverlapException(discount.ProductId);
		}

		await _Repository.UpdateAsync(discount, cancellationToken);

		var data = await _Repository.GetByIdAsync(discount.Id, cancellationToken)
			?? throw new DiscountNotFoundException(discount.Id);
		return DiscountDto.From(data);
	}
}
