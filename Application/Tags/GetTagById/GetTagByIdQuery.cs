using MediatR;

namespace ColeccionaloYa.Application.Tags.GetTagById;

public record GetTagByIdQuery(int Id) : IRequest<TagDto>;
