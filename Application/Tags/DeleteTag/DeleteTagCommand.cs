using MediatR;

namespace ColeccionaloYa.Application.Tags.DeleteTag;

public record DeleteTagCommand(int Id) : IRequest<Unit>;
