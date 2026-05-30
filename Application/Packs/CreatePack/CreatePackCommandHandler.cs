using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Packs;
using ColeccionaloYa.Domain.Packs.Exceptions;
using ColeccionaloYa.Domain.Products;
using ColeccionaloYa.Persistence.Packs.Interfaces;
using ColeccionaloYa.Persistence.Products.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Packs.CreatePack;

public class CreatePackCommandHandler : IRequestHandler<CreatePackCommand, PackSummaryDto> {
	private readonly ICConnection _Connection;
	private readonly IProductRepository _ProductRepository;
	private readonly IPackRepository _PackRepository;

	public CreatePackCommandHandler(
		ICConnection connection,
		IProductRepository productRepository,
		IPackRepository packRepository) {
		_Connection = connection;
		_ProductRepository = productRepository;
		_PackRepository = packRepository;
	}

	public async Task<PackSummaryDto> Handle(CreatePackCommand request, CancellationToken cancellationToken) {
		var productIds = request.Items.Select(i => i.IdProduct).ToList();
		if (!await _PackRepository.AllValidNonPackProductsAsync(productIds, cancellationToken)) {
			throw new InvalidPackItemException();
		}

		var product = Product.Create(
			request.Name, request.ShortDescription, request.LongDescription,
			request.Price, ProductType.Pack, request.Weight, false);

		var totalCount = request.Items.Sum(i => i.Quantity);

		await _Connection.BeginTransaction(cancellationToken);
		try {
			await _ProductRepository.CreateAsync(product, cancellationToken);

			var pack = Pack.Create(product.Id, totalCount);
			await _PackRepository.CreateAsync(pack, cancellationToken);

			await _PackRepository.AddItemsAsync(
				product.Id,
				request.Items.Select(i => (i.IdProduct, i.Quantity)).ToList(),
				cancellationToken);

			await _Connection.CommitTransaction(cancellationToken);

			return new PackSummaryDto(
				product.Id,
				totalCount,
				pack.CreationDate,
				request.Items.Select(i => new PackItemDto(i.IdProduct, i.Quantity)).ToList());
		} catch {
			await _Connection.CancelTransaction(cancellationToken);
			throw;
		}
	}
}
