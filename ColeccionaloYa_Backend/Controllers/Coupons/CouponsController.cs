using System.Security.Claims;
using ColeccionaloYa.Application.Coupons;
using ColeccionaloYa.Application.Coupons.ActivateCoupon;
using ColeccionaloYa.Application.Coupons.AssignCouponClients;
using ColeccionaloYa.Application.Coupons.CreateCoupon;
using ColeccionaloYa.Application.Coupons.DeactivateCoupon;
using ColeccionaloYa.Application.Coupons.DeleteCoupon;
using ColeccionaloYa.Application.Coupons.GetCouponById;
using ColeccionaloYa.Application.Coupons.GetCoupons;
using ColeccionaloYa.Application.Coupons.GetMyCoupons;
using ColeccionaloYa.Application.Coupons.RemoveCouponClient;
using ColeccionaloYa.Application.Coupons.UpdateCoupon;
using ColeccionaloYa.Application.Coupons.ValidateCoupon;
using ColeccionaloYa.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Coupons;

[Route("api/coupons")]
[ApiController]
[Authorize]
public class CouponsController : ControllerBase {
	private readonly IMediator _Mediator;

	public CouponsController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	[Authorize(Roles = "Admin")]
	public Task<PagedResult<CouponListItemDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery(Name = "is_active")] bool? isActive = null,
		[FromQuery] string? search = null,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20
	) =>
		_Mediator.Send(new GetCouponsQuery(page, limit, isActive, search), cancellationToken);

	[HttpGet("me")]
	public Task<List<CouponMeDto>> GetMine(CancellationToken cancellationToken) =>
		_Mediator.Send(new GetMyCouponsQuery(GetCurrentClientId()), cancellationToken);

	[HttpPost("validate")]
	public Task<CouponValidateDto> Validate([FromBody] ValidateCouponRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new ValidateCouponCommand(GetCurrentClientId(), request.Token), cancellationToken);

	[HttpGet("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<CouponDetailDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetCouponByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] CreateCouponRequest request, CancellationToken cancellationToken) {
		var coupon = await _Mediator.Send(new CreateCouponCommand(
			request.Name, request.Token, request.Description, request.ValidFrom, request.ValidUntil, request.Porcentage), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = coupon.IdCoupon }, coupon);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<CouponDto> Update(int id, [FromBody] UpdateCouponRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateCouponCommand(id, request.Name, request.Description, request.ValidFrom, request.ValidUntil, request.Porcentage), cancellationToken);

	[HttpPatch("{id:int}/activate")]
	[Authorize(Roles = "Admin")]
	public Task<CouponDto> Activate(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new ActivateCouponCommand(id), cancellationToken);

	[HttpPatch("{id:int}/deactivate")]
	[Authorize(Roles = "Admin")]
	public Task<CouponDto> Deactivate(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new DeactivateCouponCommand(id), cancellationToken);

	[HttpPost("{id:int}/clients")]
	[Authorize(Roles = "Admin")]
	public Task<CouponClientsDto> AssignClients(int id, [FromBody] AssignCouponClientsRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new AssignCouponClientsCommand(id, request.ClientIds), cancellationToken);

	[HttpDelete("{id:int}/clients/{idClient:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> RemoveClient(int id, int idClient, CancellationToken cancellationToken) {
		await _Mediator.Send(new RemoveCouponClientCommand(id, idClient), cancellationToken);
		return NoContent();
	}

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteCouponCommand(id), cancellationToken);
		return NoContent();
	}

	private int GetCurrentClientId() =>
		int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
