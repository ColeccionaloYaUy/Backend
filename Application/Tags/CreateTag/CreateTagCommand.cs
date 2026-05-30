using MediatR;

namespace ColeccionaloYa.Application.Tags.CreateTag;

public record CreateTagCommand(string Name, bool IsFranchise) : IRequest<TagDto>;
