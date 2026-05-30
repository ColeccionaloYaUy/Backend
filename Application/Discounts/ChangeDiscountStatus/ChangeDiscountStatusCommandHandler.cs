using ColeccionaloYa.Domain.Discounts.Exceptions;
using ColeccionaloYa.Persistence.Discounts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Discounts.ChangeDiscountStatus;

public class ChangeDiscountStatusCommandHandler : IRequestHandler<ChangeDiscountStatusCommand, DiscountDto> {
	private readonly IDiscountRepository _Repository;

	public ChangeDiscountStatusCommandHandler(IDiscountRepository repository) {
		_Repository = repository;
	}

	public async Task<DiscountDto> Handle(ChangeDiscountStatusCommand request, CancellationToken cancellationToken) {
		var discount = await _Repository.GetEntityAsync(request.Id, cancellationToken)
			?? throw new DiscountNotFoundException(request.Id);

		var activate = string.Equals(request.Action, "activate", StringComparison.OrdinalIgnoreCase);
		if (!activate && !string.Equals(request.Action, "deactivate", StringComparison.OrdinalIgnoreCase)) {
			throw new InvalidDiscountActionException(request.Action);
		}

		if (activate) {
			if (await _Repository.HasOverlappingActiveAsync(discount.ProductId, discount.ValidFrom, discount.ValidUntil, discount.Id, cancellationToken)) {
				throw new DiscountOverlapException(discount.ProductId);
			}
			discount.Activate();
		} else {
			discount.Deactivate();
		}

		await _Repository.UpdateAsync(discount, cancellationToken);

		var data = await _Repository.GetByIdAsync(discount.Id, cancellationToken)
			?? throw new DiscountNotFoundException(discount.Id);
		return DiscountDto.From(data);
	}
}
