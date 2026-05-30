using System.Security.Claims;
using ColeccionaloYa.Application.Payments;
using ColeccionaloYa.Application.Payments.GetOrderPayments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Payments;

[Route("api/orders/{idOrder:int}/payments")]
[ApiController]
[Authorize]
public class OrderPaymentsController : ControllerBase {
	private readonly IMediator _Mediator;

	public OrderPaymentsController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<List<PaymentDto>> GetForOrder(int idOrder, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetOrderPaymentsQuery(idOrder, GetCurrentClientId(), IsAdmin()), cancellationToken);

	private int GetCurrentClientId() =>
		int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

	private bool IsAdmin() =>
		string.Equals(User.FindFirstValue(ClaimTypes.Role), "Admin", StringComparison.Ordinal);
}
