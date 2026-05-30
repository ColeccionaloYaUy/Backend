using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Gallery;

public class ReorderGalleryRequest {
	[Required]
	[MinLength(1)]
	public List<ReorderGalleryItemRequest> Items { get; set; } = new();
}

public class ReorderGalleryItemRequest {
	public int IdGallery { get; set; }
	public int Order { get; set; }
}
