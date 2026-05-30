using Microsoft.AspNetCore.Http;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Gallery;

public class AddGalleryImageForm {
	public IFormFile? File { get; set; }
	public string? Url { get; set; }
	public int? Order { get; set; }
}
