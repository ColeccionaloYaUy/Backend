using MediatR;

namespace ColeccionaloYa.Application.Authors.UpdateAuthor;

public record UpdateAuthorCommand(int Id, string Name) : IRequest<AuthorDto>;
