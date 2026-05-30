using ColeccionaloYa.Domain.Packs.Exceptions;
using ColeccionaloYa.Persistence.Packs.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Packs.UpdatePackProduct;

public class UpdatePackProductCommandHandler : IRequestHandler<UpdatePackProductCommand, PackDetailDto> {
	private readonly IPackRepository _PackRepository;

	public UpdatePackProductCommandHandler(IPackRepository packRepository) {
		_PackRepository = packRepository;
	}

	public async Task<PackDetailDto> Handle(UpdatePackProductCommand request, CancellationToken cancellationToken) {
		if (!await _PackRepository.ExistsAsync(request.PackId, cancellationToken)) {
			throw new PackNotFoundException(request.PackId);
		}

		if (!await _PackRepository.ItemExistsAsync(request.PackId, request.ProductId, cancellationToken)) {
			throw new PackItemNotFoundException(request.PackId, request.ProductId);
		}

		await _PackRepository.UpdateItemAsync(request.PackId, request.ProductId, request.Quantity, cancellationToken);
		await _PackRepository.RecalculateCountAsync(request.PackId, cancellationToken);

		var data = await _PackRepository.GetDetailAsync(request.PackId, cancellationToken)
			?? throw new PackNotFoundException(request.PackId);
		return PackDetailDto.From(data);
	}
}
