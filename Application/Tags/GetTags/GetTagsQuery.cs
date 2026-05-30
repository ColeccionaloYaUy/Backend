using MediatR;

namespace ColeccionaloYa.Application.Tags.GetTags;

public record GetTagsQuery(bool? IsFranchise, string? Search) : IRequest<List<TagDto>>;
