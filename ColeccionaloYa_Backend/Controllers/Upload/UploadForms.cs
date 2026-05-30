using Microsoft.AspNetCore.Http;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Upload;

public class UploadImageForm {
	public IFormFile File { get; set; } = default!;
}

public class UploadImagesForm {
	public List<IFormFile> Files { get; set; } = new();
}
