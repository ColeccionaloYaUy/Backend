using ColeccionaloYa.Domain.Tags.Exceptions;
using ColeccionaloYa.Persistence.Tags.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Tags.DeleteTag;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Unit> {
	private readonly ITagRepository _Repository;

	public DeleteTagCommandHandler(ITagRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken) {
		var tag = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new TagNotFoundException(request.Id);

		if (await _Repository.IsUsedAsync(request.Id, cancellationToken)) {
			throw new TagInUseException(request.Id);
		}

		await _Repository.DeleteAsync(tag.Id, cancellationToken);
		return Unit.Value;
	}
}
