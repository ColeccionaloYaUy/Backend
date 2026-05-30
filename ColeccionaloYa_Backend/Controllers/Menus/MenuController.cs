using ColeccionaloYa.Application.Menus;
using ColeccionaloYa.Application.Menus.CreateMenuItem;
using ColeccionaloYa.Application.Menus.DeleteMenuItem;
using ColeccionaloYa.Application.Menus.GetMenu;
using ColeccionaloYa.Application.Menus.GetMenuItemById;
using ColeccionaloYa.Application.Menus.ReorderMenu;
using ColeccionaloYa.Application.Menus.UpdateMenuItem;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Menus;

[Route("api/menu")]
[ApiController]
public class MenuController : ControllerBase {
	private readonly IMediator _Mediator;

	public MenuController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<List<MenuTreeDto>> GetTree(CancellationToken cancellationToken) =>
		_Mediator.Send(new GetMenuQuery(), cancellationToken);

	[HttpGet("{id:int}")]
	public Task<MenuItemDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetMenuItemByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] CreateMenuRequest request, CancellationToken cancellationToken) {
		var item = await _Mediator.Send(new CreateMenuItemCommand(
			request.Name, request.IdTag, request.Order, request.IdMenuReferenced, request.IsCategoryFilter), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = item.IdMenu }, item);
	}

	[HttpPut("reorder")]
	[Authorize(Roles = "Admin")]
	public Task<List<MenuTreeDto>> Reorder([FromBody] ReorderMenuRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new ReorderMenuCommand(
			request.Items.Select(i => new ReorderMenuItem(i.IdMenu, i.Order)).ToList()), cancellationToken);

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<MenuItemDto> Update(int id, [FromBody] UpdateMenuRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateMenuItemCommand(
			id, request.Name, request.IdTag, request.Order, request.IdMenuReferenced, request.IsCategoryFilter), cancellationToken);

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteMenuItemCommand(id), cancellationToken);
		return NoContent();
	}
}
