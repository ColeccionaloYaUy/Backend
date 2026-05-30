using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Authors;

public class AuthorRequest {
	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;
}
