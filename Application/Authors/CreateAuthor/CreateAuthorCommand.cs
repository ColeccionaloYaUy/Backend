using MediatR;

namespace ColeccionaloYa.Application.Authors.CreateAuthor;

public record CreateAuthorCommand(string Name) : IRequest<AuthorDto>;
