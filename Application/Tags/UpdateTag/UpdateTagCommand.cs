using MediatR;

namespace ColeccionaloYa.Application.Tags.UpdateTag;

public record UpdateTagCommand(int Id, string Name, bool IsFranchise) : IRequest<TagDto>;
