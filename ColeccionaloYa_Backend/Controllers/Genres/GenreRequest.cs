using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Genres;

public class GenreRequest {
	[Required]
	[MaxLength(20)]
	public string Name { get; set; } = string.Empty;
}
