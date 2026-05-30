using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Application.Stock;
using ColeccionaloYa.Application.Stock.CreateStockMovement;
using ColeccionaloYa.Application.Stock.GetProductStock;
using ColeccionaloYa.Application.Stock.GetProductStockMovements;
using ColeccionaloYa.Application.Stock.GetStockMovements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Stock;

[Route("api/stock")]
[ApiController]
[Authorize(Roles = "Admin")]
public class StockController : ControllerBase {
	private readonly IMediator _Mediator;

	public StockController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<PagedResult<StockMovementDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery(Name = "id_product")] int? productId = null,
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20
	) =>
		_Mediator.Send(new GetStockMovementsQuery(page, limit, productId, dateFrom, dateTo), cancellationToken);

	[HttpGet("product/{idProduct:int}")]
	public Task<StockSummaryDto> GetProductStock(int idProduct, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetProductStockQuery(idProduct), cancellationToken);

	[HttpGet("product/{idProduct:int}/movements")]
	public Task<PagedResult<StockMovementHistoryDto>> GetProductMovements(
		int idProduct,
		CancellationToken cancellationToken,
		[FromQuery(Name = "date_from")] DateOnly? dateFrom = null,
		[FromQuery(Name = "date_to")] DateOnly? dateTo = null,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20
	) =>
		_Mediator.Send(new GetProductStockMovementsQuery(idProduct, page, limit, dateFrom, dateTo), cancellationToken);

	[HttpPost("movement")]
	public async Task<IActionResult> CreateMovement([FromBody] CreateStockMovementRequest request, CancellationToken cancellationToken) {
		var movement = await _Mediator.Send(new CreateStockMovementCommand(
			request.IdProduct, request.Type, request.Quantity, request.Date, request.IdOrder, request.Reason), cancellationToken);
		return StatusCode(StatusCodes.Status201Created, movement);
	}
}
