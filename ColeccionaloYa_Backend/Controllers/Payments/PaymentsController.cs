using System.Security.Claims;
using ColeccionaloYa.Application.Payments;
using ColeccionaloYa.Application.Payments.CancelPayment;
using ColeccionaloYa.Application.Payments.CreatePreference;
using ColeccionaloYa.Application.Payments.GetOrderPaymentStatus;
using ColeccionaloYa.Application.Payments.GetPaymentById;
using ColeccionaloYa.Application.Payments.GetPayments;
using ColeccionaloYa.Application.Payments.ProcessWebhook;
using ColeccionaloYa.Application.Payments.RefundPayment;
using ColeccionaloYa.Application.Payments.RetryPayment;
using ColeccionaloYa.Application.Payments.SyncPayment;
using ColeccionaloYa.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Payments;

[Route("api/payments")]
[ApiController]
[Authorize]
public class PaymentsController : ControllerBase {
	private readonly IMediator _Mediator;

	public PaymentsController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	[Authorize(Roles = "Admin")]
	public Task<PagedResult<PaymentListItemDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery] string? status = null,
		[FromQuery(Name = "id_order")] int? orderId = null,
		[FromQuery(Name = "id_client")] int? clientId = null,
		[FromQuery] string? provider = null,
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20
	) =>
		_Mediator.Send(new GetPaymentsQuery(page, limit, status, orderId, clientId, provider, dateFrom, dateTo), cancellationToken);

	[HttpGet("{idPayment:int}")]
	[Authorize(Roles = "Admin")]
	public Task<PaymentDetailDto> GetById(int idPayment, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetPaymentByIdQuery(idPayment), cancellationToken);

	[HttpPost("create-preference")]
	public async Task<IActionResult> CreatePreference([FromBody] CreatePreferenceRequest request, CancellationToken cancellationToken) {
		var result = await _Mediator.Send(new CreatePreferenceCommand(request.IdOrder, GetCurrentClientId(), IsAdmin()), cancellationToken);
		return StatusCode(StatusCodes.Status201Created, result);
	}

	[HttpPost("webhook")]
	[AllowAnonymous]
	public async Task<IActionResult> Webhook([FromBody] PaymentWebhookRequest request, CancellationToken cancellationToken) {
		await _Mediator.Send(new ProcessWebhookCommand(request.Data?.Id), cancellationToken);
		return Ok(new { received = true });
	}

	[HttpGet("{idOrder:int}/status")]
	public Task<PaymentStatusResultDto> GetOrderStatus(int idOrder, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetOrderPaymentStatusQuery(idOrder, GetCurrentClientId(), IsAdmin()), cancellationToken);

	[HttpPost("{idPayment:int}/refund")]
	[Authorize(Roles = "Admin")]
	public Task<RefundResultDto> Refund(int idPayment, [FromBody] RefundPaymentRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new RefundPaymentCommand(idPayment, request.Amount, request.Reason), cancellationToken);

	[HttpPost("{idPayment:int}/cancel")]
	[Authorize(Roles = "Admin")]
	public Task<CancelResultDto> Cancel(int idPayment, [FromBody] CancelPaymentRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new CancelPaymentCommand(idPayment, request.Reason), cancellationToken);

	[HttpPost("{idPayment:int}/retry")]
	public async Task<IActionResult> Retry(int idPayment, CancellationToken cancellationToken) {
		var result = await _Mediator.Send(new RetryPaymentCommand(idPayment, GetCurrentClientId(), IsAdmin()), cancellationToken);
		return StatusCode(StatusCodes.Status201Created, result);
	}

	[HttpPost("{idPayment:int}/sync")]
	[Authorize(Roles = "Admin")]
	public Task<SyncResultDto> Sync(int idPayment, CancellationToken cancellationToken) =>
		_Mediator.Send(new SyncPaymentCommand(idPayment), cancellationToken);

	private int GetCurrentClientId() =>
		int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

	private bool IsAdmin() =>
		string.Equals(User.FindFirstValue(ClaimTypes.Role), "Admin", StringComparison.Ordinal);
}
