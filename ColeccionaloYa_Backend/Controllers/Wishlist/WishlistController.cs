using System.Security.Claims;
using ColeccionaloYa.Application.Wishlist;
using ColeccionaloYa.Application.Wishlist.AddToWishlist;
using ColeccionaloYa.Application.Wishlist.CheckWishlist;
using ColeccionaloYa.Application.Wishlist.GetWishlist;
using ColeccionaloYa.Application.Wishlist.RemoveFromWishlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Wishlist;

[Route("api/wishlist")]
[ApiController]
[Authorize]
public class WishlistController : ControllerBase {
	private readonly IMediator _Mediator;

	public WishlistController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<List<WishlistItemDto>> Get(CancellationToken cancellationToken) =>
		_Mediator.Send(new GetWishlistQuery(GetCurrentClientId()), cancellationToken);

	[HttpPost("{idProduct:int}")]
	public async Task<IActionResult> Add(int idProduct, CancellationToken cancellationToken) {
		var result = await _Mediator.Send(new AddToWishlistCommand(GetCurrentClientId(), idProduct), cancellationToken);
		return StatusCode(StatusCodes.Status201Created, result);
	}

	[HttpDelete("{idProduct:int}")]
	public async Task<IActionResult> Remove(int idProduct, CancellationToken cancellationToken) {
		await _Mediator.Send(new RemoveFromWishlistCommand(GetCurrentClientId(), idProduct), cancellationToken);
		return NoContent();
	}

	[HttpGet("check/{idProduct:int}")]
	public Task<WishlistCheckDto> Check(int idProduct, CancellationToken cancellationToken) =>
		_Mediator.Send(new CheckWishlistQuery(GetCurrentClientId(), idProduct), cancellationToken);

	private int GetCurrentClientId() =>
		int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
