using ColeccionaloYa.Persistence.Carts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Carts.ValidateCart;

public class ValidateCartCommandHandler : IRequestHandler<ValidateCartCommand, CartValidationDto> {
	private readonly ICartRepository _CartRepository;

	public ValidateCartCommandHandler(ICartRepository cartRepository) {
		_CartRepository = cartRepository;
	}

	public async Task<CartValidationDto> Handle(ValidateCartCommand request, CancellationToken cancellationToken) {
		var data = await _CartRepository.GetCartDataAsync(request.ClientId, cancellationToken);
		var issues = new List<CartValidationIssueDto>();

		if (data is not null) {
			foreach (var item in data.Items) {
				if (item.CurrentStock <= 0) {
					issues.Add(new CartValidationIssueDto(
						item.IdProduct, item.Name, "out_of_stock",
						$"Product '{item.Name}' is out of stock.",
						item.Quantity, item.CurrentStock, null, null));
				} else if (item.Quantity > item.CurrentStock) {
					issues.Add(new CartValidationIssueDto(
						item.IdProduct, item.Name, "insufficient_stock",
						$"Only {item.CurrentStock} units of '{item.Name}' are available.",
						item.Quantity, item.CurrentStock, null, null));
				}
			}
		}

		return new CartValidationDto(issues.Count == 0, issues);
	}
}
