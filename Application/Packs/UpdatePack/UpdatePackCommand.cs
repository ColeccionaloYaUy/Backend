using MediatR;

namespace ColeccionaloYa.Application.Packs.UpdatePack;

public record UpdatePackCommand(
	int Id,
	string? Name,
	string? ShortDescription,
	string? LongDescription,
	decimal? Price,
	decimal? Weight
) : IRequest<PackDetailDto>;
