using ColeccionaloYa.Domain.Tags.Exceptions;
using ColeccionaloYa.Persistence.Tags.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Tags.UpdateTag;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, TagDto> {
	private readonly ITagRepository _Repository;

	public UpdateTagCommandHandler(ITagRepository repository) {
		_Repository = repository;
	}

	public async Task<TagDto> Handle(UpdateTagCommand request, CancellationToken cancellationToken) {
		var tag = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new TagNotFoundException(request.Id);

		if (await _Repository.ExistsByNameAsync(request.Name, request.Id, cancellationToken)) {
			throw new DuplicateTagNameException(request.Name);
		}

		tag.Update(request.Name, request.IsFranchise);
		await _Repository.UpdateAsync(tag, cancellationToken);
		return TagDto.From(tag);
	}
}
