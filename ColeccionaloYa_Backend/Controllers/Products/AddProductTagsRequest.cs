using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Products;

public class AddProductTagsRequest {
	[Required]
	[MinLength(1)]
	public List<int> TagIds { get; set; } = new();
}
