using MediatR;

namespace ColeccionaloYa.Application.Packs.UpdatePackProduct;

public record UpdatePackProductCommand(int PackId, int ProductId, int Quantity) : IRequest<PackDetailDto>;
