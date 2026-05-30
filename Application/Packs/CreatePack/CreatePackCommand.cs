using MediatR;

namespace ColeccionaloYa.Application.Packs.CreatePack;

public record CreatePackItem(int IdProduct, int Quantity);

public record CreatePackCommand(
	string Name,
	string ShortDescription,
	string LongDescription,
	decimal Price,
	decimal Weight,
	IReadOnlyList<CreatePackItem> Items
) : IRequest<PackSummaryDto>;
