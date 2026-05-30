using ColeccionaloYa.Domain.Discounts.Exceptions;
using ColeccionaloYa.Persistence.Discounts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Discounts.GetDiscountById;

public class GetDiscountByIdQueryHandler : IRequestHandler<GetDiscountByIdQuery, DiscountDto> {
	private readonly IDiscountRepository _Repository;

	public GetDiscountByIdQueryHandler(IDiscountRepository repository) {
		_Repository = repository;
	}

	public async Task<DiscountDto> Handle(GetDiscountByIdQuery request, CancellationToken cancellationToken) {
		var data = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new DiscountNotFoundException(request.Id);
		return DiscountDto.From(data);
	}
}
