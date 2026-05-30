using System.Security.Claims;
using ColeccionaloYa.Application.Orders;
using ColeccionaloYa.Application.Orders.CancelOrder;
using ColeccionaloYa.Application.Orders.ChangeOrderStatus;
using ColeccionaloYa.Application.Orders.CreateOrder;
using ColeccionaloYa.Application.Orders.GetMyOrders;
using ColeccionaloYa.Application.Orders.GetOrderById;
using ColeccionaloYa.Application.Orders.GetOrders;
using ColeccionaloYa.Application.Orders.GetOrderTracking;
using ColeccionaloYa.Application.Orders.Invoice;
using ColeccionaloYa.Application.Orders.ReturnOrder;
using ColeccionaloYa.Application.Orders.UpdateOrderTracking;
using ColeccionaloYa.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Orders;

[Route("api/orders")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase {
	private readonly IMediator _Mediator;

	public OrdersController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	[Authorize(Roles = "Admin")]
	public Task<PagedResult<OrderListItemDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery] string? status = null,
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null,
		[FromQuery(Name = "id_client")] int? clientId = null,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20
	) =>
		_Mediator.Send(new GetOrdersQuery(page, limit, status, dateFrom, dateTo, clientId), cancellationToken);

	[HttpGet("me")]
	public Task<PagedResult<OrderListItemDto>> GetMine(
		CancellationToken cancellationToken,
		[FromQuery] string? status = null,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20
	) =>
		_Mediator.Send(new GetMyOrdersQuery(GetCurrentClientId(), page, limit, status), cancellationToken);

	[HttpGet("{id:int}")]
	public Task<OrderDetailDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetOrderByIdQuery(id, GetCurrentClientId(), IsAdmin()), cancellationToken);

	[HttpGet("{id:int}/tracking")]
	public Task<OrderTrackingDto> GetTracking(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetOrderTrackingQuery(id, GetCurrentClientId(), IsAdmin()), cancellationToken);

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken) {
		var order = await _Mediator.Send(new CreateOrderCommand(
			GetCurrentClientId(), request.IdDeliveryAddress, request.IdOrderAddress, request.Observations), cancellationToken);
		return StatusCode(StatusCodes.Status201Created, order);
	}

	[HttpPatch("{id:int}/status")]
	[Authorize(Roles = "Admin")]
	public Task<OrderDetailDto> ChangeStatus(int id, [FromBody] ChangeOrderStatusRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new ChangeOrderStatusCommand(id, request.Status, request.Reason, GetCurrentClientId()), cancellationToken);

	[HttpPatch("{id:int}/cancel")]
	public Task<OrderDetailDto> Cancel(int id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new CancelOrderCommand(id, GetCurrentClientId(), IsAdmin(), request.Reason), cancellationToken);

	[HttpPatch("{id:int}/tracking")]
	[Authorize(Roles = "Admin")]
	public Task<OrderDetailDto> UpdateTracking(int id, [FromBody] UpdateOrderTrackingRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateOrderTrackingCommand(id, request.Tracking), cancellationToken);

	[HttpPost("{id:int}/return")]
	public Task<OrderDetailDto> Return(int id, [FromBody] ReturnOrderRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new ReturnOrderCommand(id, GetCurrentClientId(), IsAdmin(), request.Reason), cancellationToken);

	[HttpGet("{id:int}/invoice")]
	public async Task<IActionResult> GetInvoice(int id, CancellationToken cancellationToken) {
		var file = await _Mediator.Send(new GetOrderInvoiceQuery(id, GetCurrentClientId(), IsAdmin()), cancellationToken);
		return File(file.Content, "application/pdf", file.FileName);
	}

	private int GetCurrentClientId() =>
		int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

	private bool IsAdmin() =>
		string.Equals(User.FindFirstValue(ClaimTypes.Role), "Admin", StringComparison.Ordinal);
}
