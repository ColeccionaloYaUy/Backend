using ColeccionaloYa.Application.Discounts;
using ColeccionaloYa.Application.Discounts.ChangeDiscountStatus;
using ColeccionaloYa.Application.Discounts.CreateDiscount;
using ColeccionaloYa.Application.Discounts.GetDiscountById;
using ColeccionaloYa.Application.Discounts.GetDiscounts;
using ColeccionaloYa.Application.Discounts.UpdateDiscount;
using ColeccionaloYa.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Discounts;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DiscountsController : ControllerBase {
	private readonly IMediator _Mediator;

	public DiscountsController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<PagedResult<DiscountDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery(Name = "is_active")] bool? isActive = null,
		[FromQuery(Name = "id_product")] int? productId = null,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20
	) =>
		_Mediator.Send(new GetDiscountsQuery(page, limit, isActive, productId), cancellationToken);

	[HttpGet("{id:int}")]
	public Task<DiscountDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetDiscountByIdQuery(id), cancellationToken);

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateDiscountRequest request, CancellationToken cancellationToken) {
		var discount = await _Mediator.Send(new CreateDiscountCommand(
			request.IdProduct, request.Porcentage, request.ValidFrom, request.ValidUntil), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = discount.IdDiscount }, discount);
	}

	[HttpPut("{id:int}")]
	public Task<DiscountDto> Update(int id, [FromBody] UpdateDiscountRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateDiscountCommand(id, request.Porcentage, request.ValidFrom, request.ValidUntil), cancellationToken);

	[HttpPatch("{id:int}/status")]
	public Task<DiscountDto> ChangeStatus(int id, [FromBody] ChangeDiscountStatusRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new ChangeDiscountStatusCommand(id, request.Action), cancellationToken);
}
