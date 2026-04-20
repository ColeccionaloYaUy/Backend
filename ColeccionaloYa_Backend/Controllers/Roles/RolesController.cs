using ColeccionaloYa.Application.Roles;
using ColeccionaloYa.Application.Roles.CreateRole;
using ColeccionaloYa.Application.Roles.DeleteRole;
using ColeccionaloYa.Application.Roles.GetRoleById;
using ColeccionaloYa.Application.Roles.GetRoles;
using ColeccionaloYa.Application.Roles.UpdateRole;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Roles;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase {
	private readonly IMediator _Mediator;

	public RolesController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<List<RoleDto>> GetAll() =>
		_Mediator.Send(new GetRolesQuery());

	[HttpGet("{id:int}")]
	public Task<RoleDto> GetById(int id) =>
		_Mediator.Send(new GetRoleByIdQuery(id));

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] RoleRequest request) {
		var role = await _Mediator.Send(new CreateRoleCommand(request.Name, request.Description));
		return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
	}

	[HttpPut("{id:int}")]
	public Task<RoleDto> Update(int id, [FromBody] RoleRequest request) =>
		_Mediator.Send(new UpdateRoleCommand(id, request.Name, request.Description));

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id) {
		await _Mediator.Send(new DeleteRoleCommand(id));
		return NoContent();
	}
}
