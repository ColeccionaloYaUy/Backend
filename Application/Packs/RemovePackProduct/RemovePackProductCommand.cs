using MediatR;

namespace ColeccionaloYa.Application.Packs.RemovePackProduct;

public record RemovePackProductCommand(int PackId, int ProductId) : IRequest<Unit>;
