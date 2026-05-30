using ColeccionaloYa.Domain.Packs.Exceptions;
using ColeccionaloYa.Persistence.Packs.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Packs.UpdatePack;

public class UpdatePackCommandHandler : IRequestHandler<UpdatePackCommand, PackDetailDto> {
	private readonly IProductRepository _ProductRepository;
	private readonly IPackRepository _PackRepository;

	public UpdatePackCommandHandler(IProductRepository productRepository, IPackRepository packRepository) {
		_ProductRepository = productRepository;
		_PackRepository = packRepository;
	}

	public async Task<PackDetailDto> Handle(UpdatePackCommand request, CancellationToken cancellationToken) {
		if (!await _PackRepository.ExistsAsync(request.Id, cancellationToken)) {
			throw new PackNotFoundException(request.Id);
		}

		var product = await _ProductRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new PackNotFoundException(request.Id);

		product.Update(request.Name, request.ShortDescription, request.LongDescription,
			request.Price, null, request.Weight, null);

		await _ProductRepository.UpdateAsync(product, cancellationToken);

		var data = await _PackRepository.GetDetailAsync(request.Id, cancellationToken)
			?? throw new PackNotFoundException(request.Id);
		return PackDetailDto.From(data);
	}
}
