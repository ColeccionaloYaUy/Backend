using MediatR;

namespace ColeccionaloYa.Application.Packs.AddPackProduct;

public record AddPackProductCommand(int PackId, int ProductId, int Quantity) : IRequest<PackDetailDto>;
