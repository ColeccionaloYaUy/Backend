using ColeccionaloYa.Domain.Tags;
using ColeccionaloYa.Domain.Tags.Exceptions;
using ColeccionaloYa.Persistence.Tags.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Tags.CreateTag;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto> {
	private readonly ITagRepository _Repository;

	public CreateTagCommandHandler(ITagRepository repository) {
		_Repository = repository;
	}

	public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken) {
		if (await _Repository.ExistsByNameAsync(request.Name, null, cancellationToken)) {
			throw new DuplicateTagNameException(request.Name);
		}

		var tag = Tag.Create(request.Name, request.IsFranchise);
		await _Repository.CreateAsync(tag, cancellationToken);
		return TagDto.From(tag);
	}
}
