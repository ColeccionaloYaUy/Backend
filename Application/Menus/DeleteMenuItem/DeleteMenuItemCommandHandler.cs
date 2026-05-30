using ColeccionaloYa.Domain.Menus.Exceptions;
using ColeccionaloYa.Persistence.Menus.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Menus.DeleteMenuItem;

public class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand, Unit> {
	private readonly IMenuRepository _Repository;

	public DeleteMenuItemCommandHandler(IMenuRepository repository) {
		_Repository = repository;
	}

	public async Task<Unit> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken) {
		var item = await _Repository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new MenuItemNotFoundException(request.Id);

		if (await _Repository.HasChildrenAsync(request.Id, cancellationToken)) {
			throw new MenuHasChildrenException(request.Id);
		}

		await _Repository.DeleteAsync(item.Id, cancellationToken);
		return Unit.Value;
	}
}
