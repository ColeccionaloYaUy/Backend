using MediatR;

namespace ColeccionaloYa.Application.Authors.DeleteAuthor;

public record DeleteAuthorCommand(int Id) : IRequest<Unit>;
