using ColeccionaloYa.Domain.Packs.Exceptions;
using ColeccionaloYa.Persistence.Packs.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Packs.AddPackProduct;

public class AddPackProductCommandHandler : IRequestHandler<AddPackProductCommand, PackDetailDto> {
	private readonly IPackRepository _PackRepository;

	public AddPackProductCommandHandler(IPackRepository packRepository) {
		_PackRepository = packRepository;
	}

	public async Task<PackDetailDto> Handle(AddPackProductCommand request, CancellationToken cancellationToken) {
		if (!await _PackRepository.ExistsAsync(request.PackId, cancellationToken)) {
			throw new PackNotFoundException(request.PackId);
		}

		if (!await _PackRepository.IsValidNonPackProductAsync(request.ProductId, cancellationToken)) {
			throw new InvalidPackItemException();
		}

		if (await _PackRepository.ItemExistsAsync(request.PackId, request.ProductId, cancellationToken)) {
			throw new PackItemAlreadyExistsException(request.PackId, request.ProductId);
		}

		await _PackRepository.AddItemAsync(request.PackId, request.ProductId, request.Quantity, cancellationToken);
		await _PackRepository.RecalculateCountAsync(request.PackId, cancellationToken);

		var data = await _PackRepository.GetDetailAsync(request.PackId, cancellationToken)
			?? throw new PackNotFoundException(request.PackId);
		return PackDetailDto.From(data);
	}
}
