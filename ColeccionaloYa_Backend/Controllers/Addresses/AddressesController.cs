using System.Security.Claims;
using ColeccionaloYa.Application.Addresses;
using ColeccionaloYa.Application.Addresses.CreateAddress;
using ColeccionaloYa.Application.Addresses.DeleteAddress;
using ColeccionaloYa.Application.Addresses.GetAddressById;
using ColeccionaloYa.Application.Addresses.GetAddressesByClient;
using ColeccionaloYa.Application.Addresses.UpdateAddress;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Addresses;

[ApiController]
[Authorize]
public class AddressesController : ControllerBase {
	private readonly IMediator _Mediator;

	public AddressesController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet("api/clients/{clientId:int}/addresses")]
	public Task<List<AddressDto>> GetByClient(int clientId) {
		var (requesterId, isAdmin) = GetRequester();
		return _Mediator.Send(new GetAddressesByClientQuery(clientId, requesterId, isAdmin));
	}

	[HttpGet("api/addresses/{id:int}")]
	public Task<AddressDto> GetById(int id) {
		var (requesterId, isAdmin) = GetRequester();
		return _Mediator.Send(new GetAddressByIdQuery(id, requesterId, isAdmin));
	}

	[HttpPost("api/clients/{clientId:int}/addresses")]
	public async Task<IActionResult> Create(int clientId, [FromBody] AddressRequest request) {
		var (requesterId, isAdmin) = GetRequester();
		var address = await _Mediator.Send(new CreateAddressCommand(
			clientId, request.Street, request.Number, request.City, request.Department, request.PostalCode, request.Type,
			requesterId, isAdmin
		));
		return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
	}

	[HttpPut("api/addresses/{id:int}")]
	public Task<AddressDto> Update(int id, [FromBody] AddressRequest request) {
		var (requesterId, isAdmin) = GetRequester();
		return _Mediator.Send(new UpdateAddressCommand(
			id, request.Street, request.Number, request.City, request.Department, request.PostalCode, request.Type,
			requesterId, isAdmin
		));
	}

	[HttpDelete("api/addresses/{id:int}")]
	public async Task<IActionResult> Delete(int id) {
		var (requesterId, isAdmin) = GetRequester();
		await _Mediator.Send(new DeleteAddressCommand(id, requesterId, isAdmin));
		return NoContent();
	}

	private (int requesterId, bool isAdmin) GetRequester() {
		var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
		var isAdmin = User.IsInRole("Admin");
		return (id, isAdmin);
	}
}
