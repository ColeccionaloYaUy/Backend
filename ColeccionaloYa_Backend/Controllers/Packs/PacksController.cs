using ColeccionaloYa.Application.Packs;
using ColeccionaloYa.Application.Packs.AddPackProduct;
using ColeccionaloYa.Application.Packs.CreatePack;
using ColeccionaloYa.Application.Packs.GetPackById;
using ColeccionaloYa.Application.Packs.RemovePackProduct;
using ColeccionaloYa.Application.Packs.UpdatePack;
using ColeccionaloYa.Application.Packs.UpdatePackProduct;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Packs;

[Route("api/[controller]")]
[ApiController]
public class PacksController : ControllerBase {
	private readonly IMediator _Mediator;

	public PacksController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet("{id:int}")]
	public Task<PackDetailDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetPackByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] CreatePackRequest request, CancellationToken cancellationToken) {
		var pack = await _Mediator.Send(new CreatePackCommand(
			request.Name, request.ShortDescription, request.LongDescription, request.Price, request.Weight,
			request.Items.Select(i => new CreatePackItem(i.IdProduct, i.Quantity)).ToList()), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = pack.IdProduct }, pack);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<PackDetailDto> Update(int id, [FromBody] UpdatePackRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdatePackCommand(
			id, request.Name, request.ShortDescription, request.LongDescription, request.Price, request.Weight),
			cancellationToken);

	[HttpPost("{id:int}/products")]
	[Authorize(Roles = "Admin")]
	public Task<PackDetailDto> AddProduct(int id, [FromBody] AddPackProductRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new AddPackProductCommand(id, request.IdProduct, request.Quantity), cancellationToken);

	[HttpPut("{id:int}/products/{idProduct:int}")]
	[Authorize(Roles = "Admin")]
	public Task<PackDetailDto> UpdateProduct(int id, int idProduct, [FromBody] UpdatePackProductRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdatePackProductCommand(id, idProduct, request.Quantity), cancellationToken);

	[HttpDelete("{id:int}/products/{idProduct:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> RemoveProduct(int id, int idProduct, CancellationToken cancellationToken) {
		await _Mediator.Send(new RemovePackProductCommand(id, idProduct), cancellationToken);
		return NoContent();
	}
}
