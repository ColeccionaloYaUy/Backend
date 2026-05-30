using ColeccionaloYa.Domain.Packs.Exceptions;
using ColeccionaloYa.Persistence.Packs.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Packs.RemovePackProduct;

public class RemovePackProductCommandHandler : IRequestHandler<RemovePackProductCommand, Unit> {
	private readonly IPackRepository _PackRepository;

	public RemovePackProductCommandHandler(IPackRepository packRepository) {
		_PackRepository = packRepository;
	}

	public async Task<Unit> Handle(RemovePackProductCommand request, CancellationToken cancellationToken) {
		if (!await _PackRepository.ItemExistsAsync(request.PackId, request.ProductId, cancellationToken)) {
			throw new PackItemNotFoundException(request.PackId, request.ProductId);
		}

		await _PackRepository.RemoveItemAsync(request.PackId, request.ProductId, cancellationToken);
		await _PackRepository.RecalculateCountAsync(request.PackId, cancellationToken);
		return Unit.Value;
	}
}
