using ColeccionaloYa.Domain.Tags.Exceptions;
using ColeccionaloYa.Persistence.Tags.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Tags.GetTagById;

public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, TagDto> {
	private readonly ITagRepository _Repository;

	public GetTagByIdQueryHandler(ITagRepository repository) {
		_Repository = repository;
	}

	public async Task<TagDto> Handle(GetTagByIdQuery request, CancellationToken cancellationToken) {
		var tag = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new TagNotFoundException(request.Id);
		return TagDto.From(tag);
	}
}
