using MediatR;

namespace ColeccionaloYa.Application.Authors.GetAuthorById;

public record GetAuthorByIdQuery(int Id) : IRequest<AuthorDto>;
