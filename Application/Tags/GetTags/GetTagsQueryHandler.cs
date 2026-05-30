using ColeccionaloYa.Persistence.Tags.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Tags.GetTags;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagDto>> {
	private readonly ITagRepository _Repository;

	public GetTagsQueryHandler(ITagRepository repository) {
		_Repository = repository;
	}

	public async Task<List<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken) {
		var tags = await _Repository.GetAllAsync(request.IsFranchise, request.Search, cancellationToken);
		return tags.Select(TagDto.From).ToList();
	}
}
