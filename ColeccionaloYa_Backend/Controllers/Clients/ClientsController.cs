using System.Security.Claims;
using ColeccionaloYa.Application.Clients;
using ColeccionaloYa.Application.Clients.ActivateClient;
using ColeccionaloYa.Application.Clients.CreateClient;
using ColeccionaloYa.Application.Clients.DeactivateClient;
using ColeccionaloYa.Application.Clients.DeleteClient;
using ColeccionaloYa.Application.Clients.GetClientById;
using ColeccionaloYa.Application.Clients.GetClients;
using ColeccionaloYa.Application.Clients.GetMyProfile;
using ColeccionaloYa.Application.Clients.UpdateClient;
using ColeccionaloYa.Application.Clients.UpdateMyProfile;
using ColeccionaloYa.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Clients;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ClientsController : ControllerBase {
	private readonly IMediator _Mediator;

	public ClientsController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	[Authorize(Roles = "Admin")]
	public Task<PagedResult<ClientDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 20,
		[FromQuery] string? search = null,
		[FromQuery] int? roleId = null,
		[FromQuery] bool? active = null
	) =>
		_Mediator.Send(new GetClientsQuery(page, pageSize, search, roleId, active), cancellationToken);

	[HttpGet("me")]
	public Task<ClientDto> GetMyProfile(CancellationToken cancellationToken) {
		var clientId = GetCurrentClientId();
		return _Mediator.Send(new GetMyProfileQuery(clientId), cancellationToken);
	}

	[HttpGet("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<ClientDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetClientByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken cancellationToken) {
		var client = await _Mediator.Send(new CreateClientCommand(
			request.Name, request.Lastname, request.Email, request.Phone,
			request.Password, request.RoleId, request.Active
		), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
	}

	[HttpPut("me")]
	public Task<ClientDto> UpdateMyProfile([FromBody] UpdateMyProfileRequest request, CancellationToken cancellationToken) {
		var clientId = GetCurrentClientId();
		return _Mediator.Send(new UpdateMyProfileCommand(clientId, request.Name, request.Lastname, request.Phone), cancellationToken);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<ClientDto> Update(int id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateClientCommand(id, request.Name, request.Lastname, request.Phone, request.RoleId, request.Active), cancellationToken);

	[HttpPatch("{id:int}/activate")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new ActivateClientCommand(id), cancellationToken);
		return NoContent();
	}

	[HttpPatch("{id:int}/deactivate")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeactivateClientCommand(id), cancellationToken);
		return NoContent();
	}

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteClientCommand(id), cancellationToken);
		return NoContent();
	}

	private int GetCurrentClientId() =>
		int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
