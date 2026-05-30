using ColeccionaloYa.Persistence.Carts.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Carts.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto> {
	private readonly ICartRepository _Repository;

	public GetCartQueryHandler(ICartRepository repository) {
		_Repository = repository;
	}

	public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken) {
		await _Repository.EnsureCartAsync(request.ClientId, cancellationToken);
		var data = await _Repository.GetCartDataAsync(request.ClientId, cancellationToken);
		return CartDto.From(data!);
	}
}
