using ColeccionaloYa.Application.Gallery.DeleteGalleryImage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Gallery;

[Route("api/gallery")]
[ApiController]
public class GalleryController : ControllerBase {
	private readonly IMediator _Mediator;

	public GalleryController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteGalleryImageCommand(id), cancellationToken);
		return NoContent();
	}
}
