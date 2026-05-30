using System.Security.Claims;
using ColeccionaloYa.Application.Carts;
using ColeccionaloYa.Application.Carts.AddCartItem;
using ColeccionaloYa.Application.Carts.ApplyCoupon;
using ColeccionaloYa.Application.Carts.ClearCart;
using ColeccionaloYa.Application.Carts.GetCart;
using ColeccionaloYa.Application.Carts.RemoveCartCoupon;
using ColeccionaloYa.Application.Carts.RemoveCartItem;
using ColeccionaloYa.Application.Carts.UpdateCartItem;
using ColeccionaloYa.Application.Carts.ValidateCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Carts;

[Route("api/cart")]
[ApiController]
[Authorize]
public class CartController : ControllerBase {
	private readonly IMediator _Mediator;

	public CartController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<CartDto> Get(CancellationToken cancellationToken) =>
		_Mediator.Send(new GetCartQuery(GetCurrentClientId()), cancellationToken);

	[HttpPost("items")]
	public Task<CartDto> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new AddCartItemCommand(GetCurrentClientId(), request.IdProduct, request.Quantity), cancellationToken);

	[HttpPut("items/{idProduct:int}")]
	public Task<CartDto> UpdateItem(int idProduct, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateCartItemCommand(GetCurrentClientId(), idProduct, request.Quantity), cancellationToken);

	[HttpDelete("items/{idProduct:int}")]
	public Task<CartDto> RemoveItem(int idProduct, CancellationToken cancellationToken) =>
		_Mediator.Send(new RemoveCartItemCommand(GetCurrentClientId(), idProduct), cancellationToken);

	[HttpDelete]
	public async Task<IActionResult> Clear(CancellationToken cancellationToken) {
		await _Mediator.Send(new ClearCartCommand(GetCurrentClientId()), cancellationToken);
		return NoContent();
	}

	[HttpPost("validate")]
	public Task<CartValidationDto> Validate(CancellationToken cancellationToken) =>
		_Mediator.Send(new ValidateCartCommand(GetCurrentClientId()), cancellationToken);

	[HttpPost("apply-coupon")]
	public Task<ApplyCouponResultDto> ApplyCoupon([FromBody] ApplyCouponRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new ApplyCouponCommand(GetCurrentClientId(), request.CouponToken), cancellationToken);

	[HttpDelete("coupon")]
	public Task<CartDto> RemoveCoupon(CancellationToken cancellationToken) =>
		_Mediator.Send(new RemoveCartCouponCommand(GetCurrentClientId()), cancellationToken);

	private int GetCurrentClientId() =>
		int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
