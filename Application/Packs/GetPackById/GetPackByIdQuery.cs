using MediatR;

namespace ColeccionaloYa.Application.Packs.GetPackById;

public record GetPackByIdQuery(int Id) : IRequest<PackDetailDto>;
