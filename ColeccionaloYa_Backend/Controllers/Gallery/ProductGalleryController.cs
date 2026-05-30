using ColeccionaloYa.Application.Gallery.AddGalleryImage;
using ColeccionaloYa.Application.Gallery.GetProductGallery;
using ColeccionaloYa.Application.Gallery.ReorderGallery;
using ColeccionaloYa.Application.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Gallery;

[Route("api/products/{id:int}/gallery")]
[ApiController]
public class ProductGalleryController : ControllerBase {
	private readonly IMediator _Mediator;

	public ProductGalleryController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<List<GalleryImageDto>> GetAll(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetProductGalleryQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> Add(int id, [FromForm] AddGalleryImageForm form, CancellationToken cancellationToken) {
		byte[]? bytes = null;
		string? fileName = null;
		if (form.File is not null) {
			using var ms = new MemoryStream();
			await form.File.CopyToAsync(ms, cancellationToken);
			bytes = ms.ToArray();
			fileName = form.File.FileName;
		}

		var image = await _Mediator.Send(new AddGalleryImageCommand(id, bytes, fileName, form.Url, form.Order), cancellationToken);
		return CreatedAtAction(nameof(GetAll), new { id }, image);
	}

	[HttpPut("reorder")]
	[Authorize(Roles = "Admin")]
	public Task<List<GalleryImageDto>> Reorder(int id, [FromBody] ReorderGalleryRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new ReorderGalleryCommand(
			id,
			request.Items.Select(i => new GalleryReorderItem(i.IdGallery, i.Order)).ToList()),
			cancellationToken);
}
