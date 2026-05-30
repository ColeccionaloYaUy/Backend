using ColeccionaloYa.Application.Upload;
using ColeccionaloYa.Application.Upload.DeleteUpload;
using ColeccionaloYa.Application.Upload.UploadImage;
using ColeccionaloYa.Application.Upload.UploadImages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Upload;

[Route("api/upload")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UploadController : ControllerBase {
	private readonly IMediator _Mediator;

	public UploadController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpPost("image")]
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> UploadImage([FromForm] UploadImageForm form, CancellationToken cancellationToken) {
		var bytes = await ReadBytes(form.File, cancellationToken);
		var result = await _Mediator.Send(new UploadImageCommand(bytes, form.File.FileName), cancellationToken);
		return StatusCode(StatusCodes.Status201Created, result);
	}

	[HttpPost("images")]
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> UploadImages([FromForm] UploadImagesForm form, CancellationToken cancellationToken) {
		var items = new List<UploadFileItem>();
		foreach (var file in form.Files) {
			var bytes = await ReadBytes(file, cancellationToken);
			items.Add(new UploadFileItem(bytes, file.FileName));
		}

		var result = await _Mediator.Send(new UploadImagesCommand(items), cancellationToken);
		return StatusCode(StatusCodes.Status201Created, result);
	}

	[HttpDelete("{filename}")]
	public async Task<IActionResult> Delete(string filename, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteUploadCommand(filename), cancellationToken);
		return NoContent();
	}

	private static async Task<byte[]> ReadBytes(IFormFile file, CancellationToken cancellationToken) {
		using var ms = new MemoryStream();
		await file.CopyToAsync(ms, cancellationToken);
		return ms.ToArray();
	}
}
