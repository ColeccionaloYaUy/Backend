using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Tags;

public class UpdateTagRequest {
	[Required]
	[MaxLength(20)]
	public string Name { get; set; } = string.Empty;

	public bool IsFranchise { get; set; }
}
